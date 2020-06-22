using System;

namespace NETMetaCoder.TestApp
{
    public class TestingGround
    {
        public void TestingGroundMethod()
        {
            try
            {

                throw new Exception("Just decided to throw an exception after calling <METHOD NAME HERE>.");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Threw an exception just for fun: {exception.Message}.");

                // return __result;
            }
        }
    }
}
