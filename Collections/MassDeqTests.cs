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
    public void EnqueueTail_And_Enumeration_Works()
    {
        var deq = new MassDeq<int>();
        deq.EnqueueTail(1);
        deq.EnqueueTail(2);
        deq.EnqueueTail(3);

        // EnqueueTail appends at the back, so enumeration (head-to-tail) matches insertion order.
        var list = deq.ToList();
        Assert.Equal(new[] { 1, 2, 3 }, list);
        Assert.Equal(3, deq.Count);
    }

    [Fact]
    public void EnqueueHead_And_Enumeration_Works()
    {
        var deq = new MassDeq<int>();
        deq.EnqueueHead(1);
        deq.EnqueueHead(2);
        deq.EnqueueHead(3);

        // EnqueueHead prepends at the front, so enumeration is the reverse of insertion order.
        var list = deq.ToList();
        Assert.Equal(new[] { 3, 2, 1 }, list);
        Assert.Equal(3, deq.Count);
    }

    [Fact]
    public void TryDequeueHead_RemovesFromHead()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b");
        deq.EnqueueTail("c");

        // Deque is: a (head), b, c (tail)

        string item;
        Assert.True(deq.TryDequeueHead(out item));
        Assert.Equal("a", item);
        Assert.Equal(2, deq.Count);

        Assert.True(deq.TryDequeueHead(out item));
        Assert.Equal("b", item);
        Assert.True(deq.TryDequeueHead(out item));
        Assert.Equal("c", item);

        Assert.False(deq.TryDequeueHead(out item));
        Assert.Equal(default, item);
        Assert.Empty(deq);
    }

    [Fact]
    public void TryDequeueTail_RemovesFromTail()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b");
        deq.EnqueueTail("c");

        // Deque is: a (head), b, c (tail)

        string item;
        Assert.True(deq.TryDequeueTail(out item));
        Assert.Equal("c", item);
        Assert.Equal(2, deq.Count);

        Assert.True(deq.TryDequeueTail(out item));
        Assert.Equal("b", item);
        Assert.True(deq.TryDequeueTail(out item));
        Assert.Equal("a", item);

        Assert.False(deq.TryDequeueTail(out item));
        Assert.Equal(default, item);
        Assert.Empty(deq);
    }

    [Fact]
    public void DequeueHead_ReturnsFromHead_MatchingTryDequeueHead()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b");
        deq.EnqueueTail("c"); // head="a", tail="c"

        Assert.Equal("a", deq.DequeueHead());
        Assert.Equal("b", deq.DequeueHead());
        Assert.Equal("c", deq.DequeueHead());
        Assert.Empty(deq);
    }

    [Fact]
    public void DequeueHead_ThrowsWhenEmpty()
    {
        var deq = new MassDeq<int>();
        Assert.Throws<InvalidOperationException>(() => deq.DequeueHead());
    }

    [Fact]
    public void DequeueTail_ThrowsWhenEmpty()
    {
        var deq = new MassDeq<int>();
        Assert.Throws<InvalidOperationException>(() => deq.DequeueTail());
    }

    [Fact]
    public void TryPeekHead_ReturnsNextItem_WithoutRemoving()
    {
        var deq = new MassDeq<int>();
        deq.EnqueueTail(1);
        deq.EnqueueTail(2); // head=1, tail=2

        Assert.True(deq.TryPeekHead(out int peeked));
        Assert.Equal(1, peeked); // same item TryDequeueHead would return
        Assert.Equal(2, deq.Count); // nothing removed

        Assert.True(deq.TryDequeueHead(out int dequeued));
        Assert.Equal(peeked, dequeued);
    }

    [Fact]
    public void TryPeekTail_ReturnsNextItem_WithoutRemoving()
    {
        var deq = new MassDeq<int>();
        deq.EnqueueTail(1);
        deq.EnqueueTail(2); // head=1, tail=2

        Assert.True(deq.TryPeekTail(out int peeked));
        Assert.Equal(2, peeked); // same item TryDequeueTail would return
        Assert.Equal(2, deq.Count); // nothing removed

        Assert.True(deq.TryDequeueTail(out int dequeued));
        Assert.Equal(peeked, dequeued);
    }

    [Fact]
    public void TryPeekHead_ReturnsFalse_WhenEmpty()
    {
        var deq = new MassDeq<int>();
        Assert.False(deq.TryPeekHead(out int item));
        Assert.Equal(default, item);
    }

    [Fact]
    public void TryPeekTail_ReturnsFalse_WhenEmpty()
    {
        var deq = new MassDeq<int>();
        Assert.False(deq.TryPeekTail(out int item));
        Assert.Equal(default, item);
    }

    [Fact]
    public void PeekHead_ReturnsNextItem_WithoutRemoving()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b"); // head=a, tail=b

        Assert.Equal("a", deq.PeekHead());
        Assert.Equal(2, deq.Count); // still there
        Assert.Equal("a", deq.PeekHead()); // idempotent
    }

    [Fact]
    public void PeekTail_ReturnsNextItem_WithoutRemoving()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b"); // head=a, tail=b

        Assert.Equal("b", deq.PeekTail());
        Assert.Equal(2, deq.Count); // still there
        Assert.Equal("b", deq.PeekTail()); // idempotent
    }

    [Fact]
    public void PeekHead_ThrowsWhenEmpty()
    {
        var deq = new MassDeq<int>();
        Assert.Throws<InvalidOperationException>(() => deq.PeekHead());
    }

    [Fact]
    public void PeekTail_ThrowsWhenEmpty()
    {
        var deq = new MassDeq<int>();
        Assert.Throws<InvalidOperationException>(() => deq.PeekTail());
    }

    [Fact]
    public void PeekHead_And_PeekTail_Are_Independent()
    {
        // Regression coverage for the API reshape: head- and tail-side operations no longer
        // share any mutable "which end is which" state (IsReversed/Reverse() are gone) — each
        // always means the literal physical end, unconditionally.
        var deq = new MassDeq<int>();
        deq.EnqueueTail(1);
        deq.EnqueueTail(2);
        deq.EnqueueTail(3); // head=1, tail=3

        Assert.Equal(1, deq.PeekHead());
        Assert.Equal(3, deq.PeekTail());

        Assert.Equal(3, deq.DequeueTail());
        Assert.Equal(2, deq.PeekTail());
        Assert.Equal(1, deq.PeekHead()); // unaffected by tail-side removal
    }

    [Fact]
    public void TryMassDequeue_ReturnsSegment()
    {
        var deq = new MassDeq<int>();
        // EnqueueTail 1,2,3,4 => head=1, tail=4
        for (int i = 1; i <= 4; i++) deq.EnqueueTail(i);

        // TryMassDequeue detaches a contiguous run starting from the head.
        bool result = deq.TryMassDequeue(v => v <= 3, out var segment);
        Assert.True(result);

        // segment should be 1,2,3 (head to tail, same relative order as the original)
        Assert.Equal(new[] { 1, 2, 3 }, segment.ToList());
        // deq should now have only 4 left
        Assert.Equal(new[] { 4 }, deq.ToList());
    }

    [Fact]
    public void TryMassDequeue_PredicateFalseAtHead_ReturnsFalse()
    {
        var deq = new MassDeq<int>();
        deq.EnqueueTail(1);
        deq.EnqueueTail(2); // head=1, tail=2

        // Predicate fails at head (1 is not > 1)
        Assert.False(deq.TryMassDequeue(v => v > 1, out var _));
    }

    [Fact(Timeout = 100000)]
    public async Task Enumeration_Snapshot_Is_Immune_To_Concurrent_Mutation_Async()
    {
        // The public GetEnumerator() (what foreach/ToList bind to) takes a detached snapshot
        // under one lock acquisition, so a concurrent EnqueueTail on another thread can never be
        // observed mid-enumeration: the result is exactly the count at snapshot time, not a
        // "maybe torn, maybe not" read.
        var deq = new MassDeq<int>();
        for (int i = 0; i < 100; i++)
            deq.EnqueueTail(i);

        var cts = new CancellationTokenSource();
        _ = Task.Run(async () =>
        {
            int val = 100;
            while (!cts.Token.IsCancellationRequested)
            {
                deq.EnqueueTail(val++);
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
                deq.EnqueueTail(i);
                Interlocked.Increment(ref enqueues);
            }
        });

        var dequeueTask = Task.Run(() =>
        {
            int value;
            while (dequeues < n)
            {
                if (deq.TryDequeueHead(out value))
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
        deq.EnqueueTail(1);
        deq.EnqueueTail(2); // head=1, tail=2

        using var enumerator = deq.GetEnumerator();
        Assert.True(enumerator.MoveNext());
        Assert.Equal(1, enumerator.Current);

        enumerator.Reset();
        Assert.True(enumerator.MoveNext());
        Assert.Equal(1, enumerator.Current);

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
        deq.EnqueueTail(1);
        deq.EnqueueTail(2);

        using var enumerator = deq.GetEnumerator();
        Assert.True(enumerator.MoveNext());

        Assert.Throws<InvalidOperationException>(() => enumerator.InsertBefore(99));
        Assert.Throws<InvalidOperationException>(() => enumerator.InsertAfter(99));
        Assert.Throws<InvalidOperationException>(() => enumerator.Remove());
    }

    [Fact]
    public void MassDeq_IsolationOfSegments()
    {
        var deq = new MassDeq<int>();
        for (int i = 0; i < 10; i++) deq.EnqueueTail(i);

        // Take a segment out
        Assert.True(deq.TryMassDequeue(x => x <= 5, out var segment));
        // Mutate original
        deq.EnqueueTail(42);
        // Segment should not see the new value
        Assert.DoesNotContain(42, segment.ToList());
    }

    [Fact]
    public void Clone_And_CloneReverse_And_GetReversedEnumerator_Work_As_Expected()
    {
        var deq = new MassDeq<int>();
        for (int i = 1; i <= 5; i++)
            deq.EnqueueTail(i); // head=1, tail=5

        var fwd = deq.ToList();
        Assert.Equal(new[] { 1, 2, 3, 4, 5 }, fwd);

        // Clone is a faithful copy: same head-to-tail order as the original.
        var clone = deq.Clone();
        Assert.Equal(fwd, clone.ToList());
        Assert.NotSame(deq, clone);

        // CloneReverse produces a genuinely reversed copy.
        var reversedClone = deq.CloneReverse();
        Assert.Equal(fwd.AsEnumerable().Reverse(), reversedClone.ToList());

        // GetReversedEnumerator walks the SAME deque tail-to-head without physically altering it
        // or paying for a clone.
        var reversedWalk = new List<int>();
        using (var e = deq.GetReversedEnumerator())
        {
            while (e.MoveNext())
                reversedWalk.Add(e.Current);
        }
        Assert.Equal(fwd.AsEnumerable().Reverse(), reversedWalk);
        // Original deque is untouched by any of the above.
        Assert.Equal(fwd, deq.ToList());
    }

    [Fact]
    public void InsertBefore_Works_At_Head_Tail_And_Middle()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b");
        deq.EnqueueTail("c"); // head="a", tail="c"

        // Insert "X" before "b" (should result in: a, X, b, c)
        Assert.True(deq.InsertBefore("X", s => s == "b"));
        Assert.Equal(new[] { "a", "X", "b", "c" }, deq.ToList());

        // Insert "Y" before "a", the head (should result in: Y, a, X, b, c)
        Assert.True(deq.InsertBefore("Y", s => s == "a"));
        Assert.Equal(new[] { "Y", "a", "X", "b", "c" }, deq.ToList());

        // Insert "Z" before "c", the tail (should result in: Y, a, X, b, Z, c)
        Assert.True(deq.InsertBefore("Z", s => s == "c"));
        Assert.Equal(new[] { "Y", "a", "X", "b", "Z", "c" }, deq.ToList());
    }

    [Fact]
    public void InsertAfter_Works_At_Head_Tail_And_Middle()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b");
        deq.EnqueueTail("c"); // head="a", tail="c"

        // Insert "X" after "a", the head (should result in: a, X, b, c)
        Assert.True(deq.InsertAfter("X", s => s == "a"));
        Assert.Equal(new[] { "a", "X", "b", "c" }, deq.ToList());

        // Insert "Y" after "X", the middle (should result in: a, X, Y, b, c)
        Assert.True(deq.InsertAfter("Y", s => s == "X"));
        Assert.Equal(new[] { "a", "X", "Y", "b", "c" }, deq.ToList());

        // Insert "Z" after "c", the tail (should result in: a, X, Y, b, c, Z)
        Assert.True(deq.InsertAfter("Z", s => s == "c"));
        Assert.Equal(new[] { "a", "X", "Y", "b", "c", "Z" }, deq.ToList());
    }

    [Fact]
    public void InsertAfter_ReturnsFalse_WhenPredicateMatchesNothing()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b");

        Assert.False(deq.InsertAfter("X", s => s == "missing"));
        Assert.Equal(new[] { "a", "b" }, deq.ToList());
    }

    [Fact]
    public void InsertBefore_Is_Visible_Via_GetReversedEnumerator_Too()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b");
        deq.EnqueueTail("c"); // head="a", tail="c"

        Assert.True(deq.InsertBefore("X", s => s == "b")); // a, X, b, c

        var reversedWalk = new List<string>();
        using (var e = deq.GetReversedEnumerator())
        {
            while (e.MoveNext())
                reversedWalk.Add(e.Current);
        }
        Assert.Equal(new[] { "c", "b", "X", "a" }, reversedWalk);
    }

    [Fact]
    public void MassEnqueue_TwoConcurrentUpdates_MassDequeueOnUpdatedValues()
    {
        var deq = new MassDeq<MyVal>();
        int N = 1000;

        // 1. Mass enqueue all (sequential for deterministic test)
        for (int i = 0; i < N; i++)
            deq.EnqueueTail(new MyVal { Val = 0 });

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
        deq.EnqueueTail(0); // recycled target node InsertBefore repeatedly searches for
        for (int i = 1; i <= 20; i++)
            deq.EnqueueTail(i);

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
                        deq.EnqueueTail(v);
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
        deq.EnqueueTail("a");
        deq.EnqueueTail("b");
        deq.EnqueueTail("c");

        Assert.True(deq.TryRemove("b", out string? removed));
        Assert.Equal("b", removed);
        Assert.Equal(2, deq.Count);
        Assert.DoesNotContain("b", deq.ToList());
    }

    [Fact]
    public void TryRemove_Head_WithSuccessors_OnlyRemovesThatNode()
    {
        // Regression test: MassDeqEnumerator.Remove() used to call MassDeq<T>.Clear() whenever the
        // removed node happened to be the physical head, wiping the whole deque even when other
        // nodes remained.
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b");
        deq.EnqueueTail("c"); // head=a, b, c=tail

        Assert.True(deq.TryRemove("a", out string? removed));
        Assert.Equal("a", removed);
        Assert.Equal(2, deq.Count);
        Assert.Equal(new[] { "b", "c" }, deq.ToList());
    }

    [Fact]
    public void TryRemove_Tail_WithPredecessors_OnlyRemovesThatNode()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");
        deq.EnqueueTail("b");
        deq.EnqueueTail("c"); // head=a, b, c=tail

        Assert.True(deq.TryRemove("c", out string? removed));
        Assert.Equal("c", removed);
        Assert.Equal(2, deq.Count);
        Assert.Equal(new[] { "a", "b" }, deq.ToList());
    }

    [Fact]
    public void TryRemove_LastRemainingItem_EmptiesDeque()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("solo");

        Assert.True(deq.TryRemove("solo", out string? removed));
        Assert.Equal("solo", removed);
        Assert.Empty(deq);
    }

    [Fact]
    public void TryRemove_OutParam_ReturnsFalse_WhenNotFound()
    {
        var deq = new MassDeq<string>();
        deq.EnqueueTail("a");

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
        deq.EnqueueTail(1);
        deq.EnqueueTail(2);

        Assert.IsAssignableFrom<ICollection<int>>(deq);
        Assert.IsAssignableFrom<IReadOnlyCollection<int>>(deq);
        Assert.Equal(2, ((IReadOnlyCollection<int>)deq).Count);
    }

    [Fact]
    public void IMassDeq_TryMassDequeue_And_Clone_ReturnIMassDeq()
    {
        IMassDeq<int> deq = new MassDeq<int>();
        for (int i = 1; i <= 4; i++) deq.EnqueueTail(i);

        Assert.True(deq.TryMassDequeue(v => v <= 3, out IMassDeq<int> segment));
        Assert.Equal(new[] { 1, 2, 3 }, segment.ToList());

        IMassDeq<int> clone = deq.Clone();
        Assert.Equal(deq.ToList(), clone.ToList());
        Assert.NotSame(deq, clone);
    }

    [Fact]
    public void Clone_AsFrozen_ProducesFrozenCopy_OriginalUnaffected()
    {
        var deq = new MassDeq<int>();
        deq.EnqueueTail(1);
        deq.EnqueueTail(2);
        deq.EnqueueTail(3);

        MassDeq<int> frozen = deq.Clone(asFrozen: true);

        Assert.True(frozen.IsReadOnly);
        Assert.False(deq.IsReadOnly);
        Assert.Equal(deq.ToList(), frozen.ToList());

        // The clone is frozen, not the source it was cloned from.
        deq.EnqueueTail(4);
        Assert.Equal(new[] { 1, 2, 3 }, frozen.ToList());
    }

    [Fact]
    public void CloneReverse_AsFrozen_ProducesFrozenReversedCopy()
    {
        var deq = new MassDeq<int>();
        for (int i = 1; i <= 3; i++) deq.EnqueueTail(i); // head=1, tail=3

        MassDeq<int> frozen = deq.CloneReverse(asFrozen: true);

        Assert.True(frozen.IsReadOnly);
        Assert.Equal(new[] { 3, 2, 1 }, frozen.ToList());
        Assert.Throws<InvalidOperationException>(() => frozen.EnqueueTail(99));
    }

    [Fact]
    public void FrozenDeq_MutatingMembers_AllThrow()
    {
        var frozen = new MassDeq<int>();
        frozen.EnqueueTail(1);
        frozen.EnqueueTail(2);
        frozen = frozen.Clone(asFrozen: true);

        Assert.Throws<InvalidOperationException>(() => frozen.EnqueueHead(0));
        Assert.Throws<InvalidOperationException>(() => frozen.EnqueueTail(3));
        Assert.Throws<InvalidOperationException>(() => frozen.TryDequeueHead(out _));
        Assert.Throws<InvalidOperationException>(() => frozen.TryDequeueTail(out _));
        Assert.Throws<InvalidOperationException>(() => frozen.DequeueHead());
        Assert.Throws<InvalidOperationException>(() => frozen.DequeueTail());
        Assert.Throws<InvalidOperationException>(() => frozen.TryRemove(1, out _));
        Assert.Throws<InvalidOperationException>(() => frozen.InsertBefore(99, v => v == 1));
        Assert.Throws<InvalidOperationException>(() => frozen.InsertAfter(99, v => v == 1));
        Assert.Throws<InvalidOperationException>(() => frozen.TryMassDequeue(v => true, out _));
        Assert.Throws<InvalidOperationException>(() => frozen.Clear());
        Assert.Throws<InvalidOperationException>(() => ((ICollection<int>)frozen).Add(3));

        // Nothing above actually mutated it.
        Assert.Equal(new[] { 1, 2 }, frozen.ToList());
    }

    [Fact]
    public void FrozenDeq_ReadMembers_AllWork_WithoutThrowing()
    {
        // Regression coverage: GetLiveEnumerator() used to guard on IsReadOnly, which meant
        // Contains/Clone/CloneReverse's own frozen fast-path (skip the lock, walk directly)
        // threw immediately instead of reading — freezing a deque made it unreadable through
        // those three members. The guard belongs on the mutating call sites, not the walk
        // primitive itself.
        var frozen = new MassDeq<int>();
        frozen.EnqueueTail(1);
        frozen.EnqueueTail(2);
        frozen.EnqueueTail(3);
        frozen = frozen.Clone(asFrozen: true);

        bool containsTwo = frozen.Contains(2);
        bool containsNinetyNine = frozen.Contains(99);
        Assert.True(containsTwo);
        Assert.False(containsNinetyNine);

        MassDeq<int> reclone = frozen.Clone();
        Assert.Equal(new[] { 1, 2, 3 }, reclone.ToList());
        Assert.False(reclone.IsReadOnly); // Clone() without asFrozen produces a mutable copy

        MassDeq<int> reclonedReversed = frozen.CloneReverse();
        Assert.Equal(new[] { 3, 2, 1 }, reclonedReversed.ToList());

        Assert.True(frozen.TryPeekHead(out int head));
        Assert.Equal(1, head);
        Assert.True(frozen.TryPeekTail(out int tail));
        Assert.Equal(3, tail);
        Assert.Equal(1, frozen.PeekHead());
        Assert.Equal(3, frozen.PeekTail());

        Assert.Equal(new[] { 1, 2, 3 }, frozen.ToList()); // foreach/GetEnumerator
        var reversedWalk = new List<int>();
        using (var e = frozen.GetReversedEnumerator())
        {
            while (e.MoveNext())
                reversedWalk.Add(e.Current);
        }
        Assert.Equal(new[] { 3, 2, 1 }, reversedWalk);

        Assert.Equal(3, frozen.Count);
    }

    [Fact]
    public void FrozenDeq_Clone_OfAnAlreadyFrozenSource_DoesNotThrow()
    {
        // Regression: Clone()'s frozen fast-path used to call GetLiveEnumerator() on `this`,
        // which threw when `this` was already frozen — cloning a frozen deque (e.g. to hand a
        // second independent frozen snapshot to another reader) was impossible.
        var deq = new MassDeq<int>();
        deq.EnqueueTail(1);
        deq.EnqueueTail(2);
        MassDeq<int> frozen = deq.Clone(asFrozen: true);

        MassDeq<int> frozenAgain = frozen.Clone(asFrozen: true);

        Assert.True(frozenAgain.IsReadOnly);
        Assert.Equal(frozen.ToList(), frozenAgain.ToList());
        Assert.NotSame(frozen, frozenAgain);
    }

    [Fact]
    public void FrozenDeq_StructuralFreeze_DoesNotFreezeElementFields()
    {
        // Freezing is structural (which nodes exist, and their order) not a deep/value freeze:
        // the clone holds the SAME reference-typed elements as the source, so mutating an
        // element's own field through any other reference to it is still visible through the
        // frozen clone. Documented caveat, not a bug — covering it so it doesn't get "fixed"
        // into a false promise later.
        var deq = new MassDeq<MyVal>();
        var shared = new MyVal { Val = 1 };
        deq.EnqueueTail(shared);

        MassDeq<MyVal> frozen = deq.Clone(asFrozen: true);
        Assert.Equal(1, frozen.PeekHead().Val);

        shared.Val = 2;

        Assert.Equal(2, frozen.PeekHead().Val);
    }

    private class MyVal
    {
        public int Val;
    }
}
