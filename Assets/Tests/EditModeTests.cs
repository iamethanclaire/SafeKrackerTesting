using NUnit.Framework;
using UnityEngine;
public class EditModeTests
{
    private QuestionManager questionManager;

    [SetUp]
    public void Setup()
    {
        GameObject go = new GameObject();
        questionManager = go.AddComponent<QuestionManager>();
        questionManager.numberScrollers = new NumberScroller[3] { new NumberScroller(), new NumberScroller(), new NumberScroller() };
    }

    [Test]
    public void PopulateQuestions_ShouldFill_AllQuestionsDictionary()
    {
        questionManager.PopulateQuestions();

        Assert.IsTrue(QuestionManager.AllQuestions.Count > 0);
        Assert.IsTrue(QuestionManager.AllQuestions.ContainsKey("Is this number divisible by 2?"));
    }

    [Test]
    public void IsPrime_ReturnsCorrectValues()
    {
        Assert.IsTrue(QuestionManager.IsPrime(7));
        Assert.IsFalse(QuestionManager.IsPrime(15));
        Assert.IsTrue(QuestionManager.IsPrime(23));
    }

    [Test]
    public void GenerateNewNumbers_GeneratesNumbersInRange()
    {
        questionManager.GenerateNewNumbers();
        foreach (var num in questionManager.correctNumbers)
        {
            Assert.IsTrue(num >= 10 && num < 100, $"Number {num} not in range");
        }
    }

}
