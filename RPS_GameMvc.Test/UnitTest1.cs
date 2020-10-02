using System;
using RPS_GameMvc.GamePlay;
using Xunit;

namespace RPS_GameMvc.Test
{
	public class UnitTest1
	{
		[Fact]
		public void Test1()
		{
			Assert.Equal(1,1);
		}

		[Fact]
		public void TestMeSendInt()
		{
			//Arrange
			int x = 5;
			Rps_GameMethods r = new Rps_GameMethods();

			//ACT
			int y = Rps_GameMethods.TestMeSendInt(x);

			//ASSERT
			Assert.Equal(25,y);
		}
	}
}
