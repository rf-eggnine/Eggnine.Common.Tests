// Copyright © 2025 RF@Eggnine.com
// Licensed under the MIT License. See LICENSE file in the project root.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eggnine.Common.Collections;
using Xunit;

namespace Eggnine.Common.Tests.Collections;

public class MassDeqTests
{
    [Fact]
    public void Enqueue_And_Enumeration_Works()
    {
        var deq = new MassDeq<int>();
        deq.Enqueue(1);
        deq.Enqueue(2);
        deq.Enqueue(3);

        // Remember: Enqueue adds to HEAD, so enumeration order is 3,2,1
        var list = deq.ToList();
        Assert.Equal(new[] { 3, 2, 1 }, list);
        Assert.Equal(3, deq.Count);
    }

    [Fact]
    public void TryDequeue_RemovesFromTail()
    {
        var deq = new MassDeq<string>();
        deq.Enqueue("a");
        deq.Enqueue("b");
        deq.Enqueue("c");

        // Deque is: c (head), b, a (tail)

        string item;
        Assert.True(deq.TryDequeue(out item));
        Assert.Equal("a", item); // removes from TAIL
        Assert.Equal(2, deq.Count);

        Assert.True(deq.TryDequeue(out item));
        Assert.Equal("b", item);
        Assert.True(deq.TryDequeue(out item));
        Assert.Equal("c", item);

        Assert.False(deq.TryDequeue(out item));
        Assert.Equal(default, item);
        Assert.Empty(deq);
    }

    [Fact]
    public void Dequeue_ReturnsFromTail_MatchingTryDequeue()
    {
        var deq = new MassDeq<string>();
        deq.Enqueue("a");
        deq.Enqueue("b");
        deq.Enqueue("c"); // head="c", tail="a"

        Assert.Equal("a", deq.Dequeue());
        Assert.Equal("b", deq.Dequeue());
        Assert.Equal("c", deq.Dequeue());
        Assert.Empty(deq);
    }

    [Fact]
    public void Dequeue_ThrowsWhenEmpty()
    {
        var deq = new MassDeq<int>();
        Assert.Throws<InvalidOperationException>(() => deq.Dequeue());
    }

    [Fact]
    public void TryPeek_ReturnsNextItem_WithoutRemoving()
    {
        var deq = new MassDeq<int>();
        deq.Enqueue(1);
        deq.Enqueue(2); // head=2, tail=1

        Assert.True(deq.TryPeek(out int peeked));
        Assert.Equal(1, peeked); // same item TryDequeue would return
        Assert.Equal(2, deq.Count); // nothing removed

        Assert.True(deq.TryDequeue(out int dequeued));
        Assert.Equal(peeked, dequeued);
    }

    [Fact]
    public void TryPeek_ReturnsFalse_WhenEmpty()
    {
        var deq = new MassDeq<int>();
        Assert.False(deq.TryPeek(out int item));
        Assert.Equal(default, item);
    }

    [Fact]
    public void Peek_ReturnsNextItem_WithoutRemoving()
    {
        var deq = new MassDeq<string>();
        deq.Enqueue("a");
        deq.Enqueue("b");

        Assert.Equal("a", deq.Peek());
        Assert.Equal(2, deq.Count); // still there
        Assert.Equal("a", deq.Peek()); // idempotent
    }

    [Fact]
    public void Peek_ThrowsWhenEmpty()
    {
        var deq = new MassDeq<int>();
        Assert.Throws<InvalidOperationException>(() => deq.Peek());
    }

    [Fact]
    public void Peek_RespectsReversedMode()
    {
        var deq = new MassDeq<int>();
        deq.Enqueue(1);
        deq.Enqueue(2);
        deq.Enqueue(3); // head=3, tail=1, TryDequeue order: 1,2,3

        Assert.Equal(1, deq.Peek());

        deq.Reverse(); // TryDequeue order flips
        Assert.Equal(3, deq.Peek());
        Assert.Equal(3, deq.Dequeue());
        Assert.Equal(2, deq.Peek());
    }

    [Fact]
    public void TryMassDequeue_ReturnsSegment()
    {
        var deq = new MassDeq<int>();
        // Enqueue 1,2,3,4 => head is 4, tail is 1
        for (int i = 1; i <= 4; i++) deq.Enqueue(i);

        // Predicate: value >= 2
        bool result = deq.TryMassDequeue(v => v <= 3, out var segment);
        Assert.True(result);

        // segment should be 4,3,2 (from head to tail)
        Assert.Equal(new[] { 3, 2, 1 }, segment.ToList());
        // deq should now have only 1 left
        Assert.Equal(new[] { 4 }, deq.ToList());
    }

    [Fact]
    public void TryMassDequeue_PredicateFalseAtTail_ReturnsFalse()
    {
        var deq = new MassDeq<int>();
        deq.Enqueue(1);
        deq.Enqueue(2);

        // Predicate fails at tail (1 < 2)
        Assert.False(deq.TryMassDequeue(v => v > 1, out var _));
    }

    [Fact(Timeout = 100000)]
    public async Task Enumeration_Snapshot_Is_Immune_To_Concurrent_Mutation_Async()
    {
        // The public GetEnumerator() (what foreach/ToList bind to) takes a detached snapshot
        // under one lock acquisition, so a concurrent Enqueue on another thread can never be
        // observed mid-enumeration: the result is exactly the count at snapshot time, not a
        // "maybe torn, maybe not" read.
        var deq = new MassDeq<int>();
        for (int i = 0; i < 100; i++)
            deq.Enqueue(i);

        var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            int val = 100;
            while (!cts.Token.IsCancellationRequested)
            {
                deq.Enqueue(val++);
                await Task.Delay(1);
            }
        });

        for (int i = 0; i < 5; i++)
        {
            var list = deq.ToList();
            Assert.True(list.Count >= 100);
            await Task.Delay(10);
        }

        cts.Cancel();
    }

    [Fact]
    public async Task Concurrent_Enqueue_And_Dequeue_Does_Not_Corrupt()
    {
        var deq = new MassDeq<int>();
        int enqueues = 0;
        int dequeues = 0;
        int n = 1000;
        var enqueueTask = Task.Run(() =>
        {
            for (int i = 0; i < n; i++)
            {
                deq.Enqueue(i);
                Interlocked.Increment(ref enqueues);
            }
        });

        var dequeueTask = Task.Run(() =>
        {
            int value;
            while (dequeues < n)
            {
                if (deq.TryDequeue(out value))
                    Interlocked.Increment(ref dequeues);
            }
        });
        await dequeueTask;
        await enqueueTask;

        // At the end, deque should be empty
        Assert.Empty(deq);
        Assert.False(deq.Any());
    }

    [Fact]
    public void MassDeq_Enumerator_Resets_And_Disposes_Correctly()
    {
        var deq = new MassDeq<int>();
        deq.Enqueue(1);
        deq.Enqueue(2);

        using var enumerator = deq.GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.Equal(2, enumerator.Current);

        enumerator.Reset();
        Assert.True(enumerator.MoveNext());
        Assert.Equal(2, enumerator.Current);

        enumerator.Dispose();
        Assert.Throws<InvalidOperationException>(() => { var x = enumerator.Current; });
    }

    [Fact]
    public void Public_GetEnumerator_Snapshot_Rejects_InsertBefore_And_Remove()
    {
        // Regression test for the enumeration-safety fix: the public GetEnumerator() (what
        // foreach binds to) now returns a detached snapshot, not a live view. Calling the
        // enumerator's own mutating members on that snapshot has no live list to splice into,
        // so it must throw rather than silently corrupt the (unrelated, detached) copy.
        var deq = new MassDeq<int>();
        deq.Enqueue(1);
        deq.Enqueue(2);

        using var enumerator = deq.GetEnumerator();
        Assert.True(enumerator.MoveNext());

        Assert.Throws<InvalidOperationException>(() => enumerator.InsertBefore(99));
        Assert.Throws<InvalidOperationException>(() => enumerator.Remove());
    }

    [Fact]
    public void MassDeq_IsolationOfSegments()
    {
        var deq = new MassDeq<int>();
        for (int i = 0; i < 10; i++) deq.Enqueue(i);

        // Take a segment out
        Assert.True(deq.TryMassDequeue(x => x <= 5, out var segment));
        // Mutate original
        deq.Enqueue(42);
        // Segment should not see the new value
        Assert.DoesNotContain(42, segment.ToList());
    }

    [Fact]
    public void ReverseAndCloneReverse_Works_As_Expected()
    {
        var deq = new MassDeq<int>();
        for (int i = 1; i <= 5; i++)
            deq.Enqueue(i); // head=5, tail=1

        var fwd = deq.ToList();
        Assert.Equal(new[] { 5, 4, 3, 2, 1 }, fwd);

        deq.Reverse();
        var rev = deq.ToList();
        Assert.Equal(fwd.AsEnumerable().Reverse(), rev);

        // CloneReverse creates a new deque in revers (now fwd again) order
        var clone = deq.CloneReverse();
        Assert.Equal(fwd, clone.ToList());

        // Further reversals flip the order again
        deq.Reverse();
        Assert.Equal(fwd, deq.ToList());
    }

    [Fact]
    public void InsertBefore_Works_At_Head_Tail_And_Middle()
    {
        var deq = new MassDeq<string>();
        deq.Enqueue("a");
        deq.Enqueue("b");
        deq.Enqueue("c"); // head="c", tail="a"

        // Insert "X" before "b" (should result in: c, X, b, a)
        Assert.True(deq.InsertBefore("X", s => s == "b"));
        Assert.Equal(new[] { "c", "X", "b", "a" }, deq.ToList());

        // Insert "Y" before "c" (should result in: Y, c, X, b, a)
        Assert.True(deq.InsertBefore("Y", s => s == "c"));
        Assert.Equal(new[] { "Y", "c", "X", "b", "a" }, deq.ToList());

        // Insert "Z" before "a" (should result in: Y, c, X, b, Z, a)
        Assert.True(deq.InsertBefore("Z", s => s == "a"));
        Assert.Equal(new[] { "Y", "c", "X", "b", "Z", "a" }, deq.ToList());
    }

    [Fact]
    public void InsertBefore_Works_In_Reversed_Mode()
    {
        var deq = new MassDeq<string>();
        deq.Enqueue("a");
        deq.Enqueue("b");
        deq.Enqueue("c"); // head="c", tail="a"
        deq.Reverse();

        // In reversed, enumerate: a, b, c
        Assert.Equal(new[] { "a", "b", "c" }, deq.ToList());

        // Insert "X" before "b" (should result in: a, X, b, c)
        Assert.True(deq.InsertBefore("X", s => s == "b"));
        Assert.Equal(new[] { "a", "X", "b", "c" }, deq.ToList());

        // Insert "Y" before "a" (should result in: Y, a, X, b, c)
        Assert.True(deq.InsertBefore("Y", s => s == "a"));
        Assert.Equal(new[] { "Y", "a", "X", "b", "c" }, deq.ToList());

        // Insert "Z" before "c" (should result in: Y, a, X, b, Z, c)
        Assert.True(deq.InsertBefore("Z", s => s == "c"));
        Assert.Equal(new[] { "Y", "a", "X", "b", "Z", "c" }, deq.ToList());
    }

    [Fact]
    public void MassEnqueue_TwoConcurrentUpdates_MassDequeueOnUpdatedValues()
    {
        var deq = new MassDeq<MyVal>();
        int N = 1000;

        // 1. Mass enqueue all (sequential for deterministic test)
        for (int i = 0; i < N; i++)
            deq.Enqueue(new MyVal { Val = 0 });

        // 2. Concurrently update all Val to 1
        Parallel.ForEach(deq.ToList(), item => Interlocked.Exchange(ref item.Val, 1));

        // 3. Assert all are 1
        Assert.All(deq, v => Assert.Equal(1, v.Val));

        // 4. Concurrently update all Val to 2
        Parallel.ForEach(deq.ToList(), item => Interlocked.Exchange(ref item.Val, 2));

        // 5. Assert all are 2
        Assert.All(deq, v => Assert.Equal(2, v.Val));

        // 6. MassDequeue where Val == 2 (should get all items)
        bool result = deq.TryMassDequeue(v => v.Val == 2, out var segment);
        Assert.True(result);

        Assert.Equal(N, segment.Count);
        Assert.All(segment, v => Assert.Equal(2, v.Val));

        Assert.Empty(deq);
    }


    [Fact(Timeout = 100000)]
    public async Task InsertBefore_Concurrent_With_TryMassDequeue_Never_Silently_Loses_Item()
    {
        // Regression test for a real bug: InsertBefore/TryRemove/Contains used to scan for their
        // target via an unlocked enumerator walk, then act on it under a *separate* lock. If a
        // concurrent TryMassDequeue detached the found node in between, the splice/removal would
        // "succeed" (no exception) while silently operating on a now-orphaned segment the live
        // deque no longer points to. Both scan-and-act must now happen under one lock.
        var deq = new MassDeq<int>();
        deq.Enqueue(0); // recycled target node InsertBefore repeatedly searches for
        for (int i = 1; i <= 20; i++)
            deq.Enqueue(i);

        var cts = new CancellationTokenSource();
        var confirmedInserted = new System.Collections.Concurrent.ConcurrentBag<int>();

        // Task A: continuously detach the whole list (as a Canon-fire/cancel would) and
        // re-enqueue it, so the "0" node is constantly cycling through detached states.
        Task churner = Task.Run(async () =>
        {
            while (!cts.Token.IsCancellationRequested)
            {
                if (deq.TryMassDequeue(_ => true, out MassDeq<int> segment))
                {
                    foreach (int v in segment)
                        deq.Enqueue(v);
                }
                await Task.Delay(1).ConfigureAwait(false);
            }
        });

        // Task B: continuously insert a new unique marker next to the recycled "0" node, as
        // RiffTransmitAsync does relative to `before`.
        Task inserter = Task.Run(async () =>
        {
            int nextId = 1000;
            while (!cts.Token.IsCancellationRequested)
            {
                int id = nextId++;
                if (deq.InsertBefore(id, v => v == 0))
                {
                    confirmedInserted.Add(id);
                }
                await Task.Delay(1).ConfigureAwait(false);
            }
        });

        await Task.Delay(TimeSpan.FromSeconds(2));
        cts.Cancel();
        await Task.WhenAll(churner, inserter);

        List<int> finalContents = deq.ToList();
        foreach (int id in confirmedInserted)
        {
            Assert.True(finalContents.Contains(id), $"InsertBefore reported success for {id} but it is not reachable in the live deque.");
        }
    }

    [Fact]
    public void TryRemove_OutParam_ReturnsRemovedValue()
    {
        var deq = new MassDeq<string>();
        deq.Enqueue("a");
        deq.Enqueue("b");
        deq.Enqueue("c");

        Assert.True(deq.TryRemove("b", out string? removed));
        Assert.Equal("b", removed);
        Assert.Equal(2, deq.Count);
        Assert.DoesNotContain("b", deq.ToList());
    }

    [Fact]
    public void TryRemove_Head_WithSuccessors_OnlyRemovesThatNode()
    {
        // Regression test: MassDeqEnumerator.Remove() used to call MassDeq<T>.Clear() whenever the
        // removed node happened to be the physical head, wiping the whole deque (and silently
        // resetting IsReversed) even when other nodes remained.
        var deq = new MassDeq<string>();
        deq.Enqueue("a");
        deq.Enqueue("b");
        deq.Enqueue("c"); // head=c, b, a=tail

        Assert.True(deq.TryRemove("c", out string? removed));
        Assert.Equal("c", removed);
        Assert.Equal(2, deq.Count);
        Assert.Equal(new[] { "b", "a" }, deq.ToList());
    }

    [Fact]
    public void TryRemove_Tail_WithPredecessors_OnlyRemovesThatNode()
    {
        var deq = new MassDeq<string>();
        deq.Enqueue("a");
        deq.Enqueue("b");
        deq.Enqueue("c"); // head=c, b, a=tail

        Assert.True(deq.TryRemove("a", out string? removed));
        Assert.Equal("a", removed);
        Assert.Equal(2, deq.Count);
        Assert.Equal(new[] { "c", "b" }, deq.ToList());
    }

    [Fact]
    public void TryRemove_LastRemainingItem_EmptiesDeque_WithoutResettingIsReversed()
    {
        var deq = new MassDeq<string>();
        deq.Enqueue("solo");
        deq.Reverse();

        Assert.True(deq.TryRemove("solo", out string? removed));
        Assert.Equal("solo", removed);
        Assert.Empty(deq);
        Assert.True(deq.IsReversed); // Remove() must not silently un-reverse via Clear()
    }

    [Fact]
    public void TryRemove_OutParam_ReturnsFalse_WhenNotFound()
    {
        var deq = new MassDeq<string>();
        deq.Enqueue("a");

        Assert.False(deq.TryRemove("missing", out string? removed));
        Assert.Equal(default, removed);
        Assert.Single(deq);
    }

    [Fact]
    public void ICollection_Remove_DelegatesToTryRemove()
    {
        ICollection<int> deq = new MassDeq<int>();
        deq.Add(1);
        deq.Add(2);

        Assert.True(deq.Remove(1));
        Assert.False(deq.Remove(99));
        Assert.Single(deq);
    }

    [Fact]
    public void IMassDeq_Implements_ICollection_And_IReadOnlyCollection()
    {
        IMassDeq<int> deq = new MassDeq<int>();
        deq.Enqueue(1);
        deq.Enqueue(2);

        Assert.IsAssignableFrom<ICollection<int>>(deq);
        Assert.IsAssignableFrom<IReadOnlyCollection<int>>(deq);
        Assert.Equal(2, ((IReadOnlyCollection<int>)deq).Count);
    }

    [Fact]
    public void IMassDeq_TryMassDequeue_And_Clone_ReturnIMassDeq()
    {
        IMassDeq<int> deq = new MassDeq<int>();
        for (int i = 1; i <= 4; i++) deq.Enqueue(i);

        Assert.True(deq.TryMassDequeue(v => v <= 3, out IMassDeq<int> segment));
        Assert.Equal(new[] { 3, 2, 1 }, segment.ToList());

        IMassDeq<int> clone = deq.Clone();
        Assert.Equal(deq.ToList(), clone.ToList());
        Assert.NotSame(deq, clone);
    }

    private class MyVal
    {
        public int Val;
    }
}
