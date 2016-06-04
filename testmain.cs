using NUnit.Framework;

[TestFixture]
public class CalculationTest 
{
		[Test]
		public void PlusCalculation() 
		{
			var a = 4;
			var b = 5;
			var c = a + b;
			Assert.AreEqual(9, c);
		}

		[Test]
		public void PlusCalculationOtherSample() 
		{
			var a = 5;
			var b = 5;
			var c = a + b;
			Assert.AreEqual(10, c);
		}
}