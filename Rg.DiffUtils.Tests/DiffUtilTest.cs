using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Rg.DiffUtils.Tests
{
    [TestFixture]
    public class DiffUtilTest
    {
        const string FirstRandomString64 = "FolmLmOpCxwD9du2OzozVYxcaftfTUeHFYBePSNBBIcP26QdRjzNdDZuwSRwwYVM";
        const string SecondRandomString64 = "6swMDvgqkZTtemiewuQubSdcnqAuUhTVXMgAmGiFNm8CMyUwNqTsF5ZTJfrjlzOZ";

        [Test]
        [TestCase(FirstRandomString64, SecondRandomString64, false, false)]
        [TestCase(FirstRandomString64, SecondRandomString64, true, false)]
        [TestCase(FirstRandomString64, SecondRandomString64, false, true)]
        [TestCase(FirstRandomString64, SecondRandomString64, true, true)]
        public void CorrectResult_CalculateDiff(string first, string second, bool allowBatching, bool detectMoves)
        {
            var result = DiffUtil.CalculateDiff(first, second, new DiffOptions
            {
                AllowBatching = allowBatching,
                DetectMoves = detectMoves
            });

            var list = new List<char>(first);

            foreach (var step in result.Steps)
            {
                switch (step.Status)
                {
                    case DiffStatus.Add:
                        list.InsertRange(step.NewStartIndex, step.Items.Select(i => i.NewValue));
                        break;
                    case DiffStatus.Remove:
                        list.RemoveRange(step.OldStartIndex, step.Items.Count);
                        break;
                    case DiffStatus.Move:
                        list.RemoveRange(step.OldStartIndex, step.Items.Count);
                        list.InsertRange(step.NewStartIndex, step.Items.Select(i => i.NewValue));
                        break;
                }
            }

            CollectionAssert.AreEqual(list, second);
        }

        [Test]
        public void Appended()
        {
            var str1 = "abc";
            var str2 = "abcd";

            var result = DiffUtil.CalculateDiff(str1, str2);

            var step = result.Steps[0];

            Assert.AreEqual(DiffStatus.Add, step.Status);
            Assert.AreEqual(3, step.NewStartIndex);
        }

        [Test]
        public void Prepended()
        {
            var str1 = "bcd";
            var str2 = "abcd";

            var result = DiffUtil.CalculateDiff(str1, str2);

            var step = result.Steps[0];

            Assert.AreEqual(DiffStatus.Add, step.Status);
            Assert.AreEqual(0, step.NewStartIndex);
        }

        [Test]
        public void RemoveFirst()
        {
            var str1 = "abcd";
            var str2 = "bcd";

            var result = DiffUtil.CalculateDiff(str1, str2);

            var step = result.Steps[0];

            Assert.AreEqual(DiffStatus.Remove, step.Status);
            Assert.AreEqual(0, step.OldStartIndex);
        }

        [Test]
        public void RemoveLast()
        {
            var str1 = "abcd";
            var str2 = "abc";

            var result = DiffUtil.CalculateDiff(str1, str2);

            var step = result.Steps[0];

            Assert.AreEqual(DiffStatus.Remove, step.Status);
            Assert.AreEqual(3, step.OldStartIndex);
        }

        [Test]
        public void MoveForward()
        {
            var str1 = "abcd";
            var str2 = "bcda";

            var result = DiffUtil.CalculateDiff(str1, str2);

            var step = result.Steps[0];

            Assert.AreEqual(DiffStatus.Move, step.Status);
            Assert.AreEqual(0, step.OldStartIndex);
            Assert.AreEqual(3, step.NewStartIndex);
        }

        [Test]
        public void MoveBackward()
        {
            var str1 = "bcda";
            var str2 = "abcd";

            var result = DiffUtil.CalculateDiff(str1, str2);

            var step = result.Steps[0];

            Assert.AreEqual(DiffStatus.Move, step.Status);
            Assert.AreEqual(3, step.OldStartIndex);
            Assert.AreEqual(0, step.NewStartIndex);
        }

        [Test]
        [TestCase("abc", "abcde", -1, 3, 2, DiffStatus.Add)]
        [TestCase("cde", "abcde", -1, 0, 2, DiffStatus.Add)]
        [TestCase("abcde", "abc", 3, -1, 2, DiffStatus.Remove)]
        [TestCase("abcde", "cde", 0, -1, 2, DiffStatus.Remove)]
        public void BatchingAddAndRemove(string str1, string str2, int oldStartIndex, int newStartIndex, int length, DiffStatus status)
        {
            var result = DiffUtil.CalculateDiff(str1, str2);

            Assert.AreEqual(1, result.Steps.Count);

            var step = result.Steps[0];

            Assert.AreEqual(status, step.Status);
            Assert.AreEqual(oldStartIndex, step.OldStartIndex);
            Assert.AreEqual(newStartIndex, step.NewStartIndex);
            Assert.AreEqual(length, step.Items.Count);
        }

        [Test]
        [TestCase("abc", "abcde", 2, DiffStatus.Add)]
        [TestCase("def", "abcdef", 3, DiffStatus.Add)]
        [TestCase("abcde", "abc", 2, DiffStatus.Remove)]
        [TestCase("abcdef", "def", 3, DiffStatus.Remove)]
        public void NoBatchingAddAndRemove(string str1, string str2, int stepsCount, DiffStatus status)
        {
            var result = DiffUtil.CalculateDiff(str1, str2, new DiffOptions
            {
                AllowBatching = false
            });

            Assert.AreEqual(stepsCount, result.Steps.Count);

            Assert.That(result.Steps.Select(s => s.Status), Is.All.EqualTo(status));
        }

        [Test]
        public void BatchingMoveForward()
        {
            var str1 = "abcde";
            var str2 = "cdeab";

            var result = DiffUtil.CalculateDiff(str1, str2);

            Assert.AreEqual(2, result.Steps.Count);

            Assert.AreEqual(DiffStatus.Move, result.Steps[0].Status);
            Assert.AreEqual(1, result.Steps[0].OldStartIndex);
            Assert.AreEqual(4, result.Steps[0].NewStartIndex);

            Assert.AreEqual(DiffStatus.Move, result.Steps[1].Status);
            Assert.AreEqual(0, result.Steps[1].OldStartIndex);
            Assert.AreEqual(3, result.Steps[1].NewStartIndex);
        }

        [Test]
        public void BatchingMoveBackward()
        {
            var str1 = "abcde";
            var str2 = "deabc";

            var result = DiffUtil.CalculateDiff(str1, str2);

            Assert.AreEqual(2, result.Steps.Count);

            Assert.AreEqual(DiffStatus.Move, result.Steps[0].Status);
            Assert.AreEqual(4, result.Steps[0].OldStartIndex);
            Assert.AreEqual(0, result.Steps[0].NewStartIndex);

            Assert.AreEqual(DiffStatus.Move, result.Steps[1].Status);
            Assert.AreEqual(4, result.Steps[1].OldStartIndex);
            Assert.AreEqual(0, result.Steps[1].NewStartIndex);
        }

        [Test]
        public void DiffResult_Items()
        {
            var str1 = "123456";
            var str2 = "523781";

            var result = DiffUtil.CalculateDiff(str1, str2);

            CollectionAssert.AreEquivalent("78", result.AddedItems.Select(i => i.NewValue));
            CollectionAssert.AreEquivalent("46", result.RemovedItems.Select(i => i.OldValue));
            CollectionAssert.AreEquivalent("15", result.MovedItems.Select(i => i.NewValue));
            CollectionAssert.AreEquivalent("23", result.NotMovedItems.Select(i => i.NewValue));
            CollectionAssert.AreEquivalent("1235", result.SameItems.Select(i => i.NewValue));
        }
    }
}
