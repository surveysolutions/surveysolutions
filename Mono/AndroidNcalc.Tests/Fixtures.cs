using System;
using NCalc;
using NCalc.Domain;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using NUnit.Framework;

namespace AndroidNcalc.Tests
{
    [TestFixture]
    public class Fixtures
    {
        [Test]
        public void ExpressionShouldEvaluate()
        {
            var expressions = new []
            {
                "2 + 3 + 5",
                "2 * 3 + 5",
                "2 * (3 + 5)",
                "2 * (2*(2*(2+1)))",
                "10 % 3",
                "true or false",
                "not true",
                "false || not (false and true)",
                "3 > 2 and 1 <= (3-2)",
                "3 % 2 != 10 % 3"
            };

            foreach (string expression in expressions)
                Console.WriteLine("{0} = {1}",
                    expression,
                    new Expression(expression).Evaluate());
        }

        [Test]
        public void ShouldParseValues()
        {
            Assert.That(123456, Is.EqualTo(new Expression("123456").Evaluate()));
            Assert.That(new DateTime(2001, 01, 01), Is.EqualTo(new Expression("#01/01/2001#").Evaluate()));
            Assert.That(123.456d, Is.EqualTo(new Expression("123.456").Evaluate()));
            Assert.That(true, Is.EqualTo(new Expression("true").Evaluate()));
            Assert.That("true", Is.EqualTo(new Expression("'true'").Evaluate()));
            Assert.That("azerty", Is.EqualTo(new Expression("'azerty'").Evaluate()));
        }

        [Test]
        public void ShouldHandleUnicode()
        {
			Assert.That(new Expression("'経済協力開発機構'").Evaluate(), Is.EqualTo("経済協力開発機構"));
			Assert.That(new Expression(@"'\u0048\u0065\u006C\u006C\u006F'").Evaluate(), Is.EqualTo("Hello"));
			Assert.That(new Expression(@"'\u3060'").Evaluate(), Is.EqualTo("だ"));
			Assert.That(new Expression(@"'\u0100'").Evaluate(), Is.EqualTo("\u0100"));
        }

        [Test]
        public void ShouldEscapeCharacters()
        {
            Assert.That("'hello'", Is.EqualTo(new Expression(@"'\'hello\''").Evaluate()));
            Assert.That(" ' hel lo ' ", Is.EqualTo(new Expression(@"' \' hel lo \' '").Evaluate()));
            Assert.That("hel\nlo", Is.EqualTo(new Expression(@"'hel\nlo'").Evaluate()));
        }

        [Test]
        public void ShouldDisplayErrorMessages()
        {
            try
            {
                new Expression("(3 + 2").Evaluate();
                Assert.Fail("Incorect expression was evaluated");
            }
            catch(EvaluationException e)
            {
                Console.WriteLine("Error catched: " + e.Message);
            }
        }

        [Test]
        public void Maths()
        {
            Assert.That(1M, Is.EqualTo(new Expression("Abs(-1)").Evaluate()));
            Assert.That(0d, Is.EqualTo(new Expression("Acos(1)").Evaluate()));
            Assert.That(0d, Is.EqualTo(new Expression("Asin(0)").Evaluate()));
            Assert.That(0d, Is.EqualTo(new Expression("Atan(0)").Evaluate()));
            Assert.That(2d, Is.EqualTo(new Expression("Ceiling(1.5)").Evaluate()));
            Assert.That(1d, Is.EqualTo(new Expression("Cos(0)").Evaluate()));
            Assert.That(1d, Is.EqualTo(new Expression("Exp(0)").Evaluate()));
            Assert.That(1d, Is.EqualTo(new Expression("Floor(1.5)").Evaluate()));
            Assert.That(-1d, Is.EqualTo(new Expression("IEEERemainder(3,2)").Evaluate()));
            Assert.That(0d, Is.EqualTo(new Expression("Log(1,10)").Evaluate()));
            Assert.That(0d, Is.EqualTo(new Expression("Log10(1)").Evaluate()));
            Assert.That(9d, Is.EqualTo(new Expression("Pow(3,2)").Evaluate()));
            Assert.That(3.22d, Is.EqualTo(new Expression("Round(3.222,2)").Evaluate()));
            Assert.That(-1, Is.EqualTo(new Expression("Sign(-10)").Evaluate()));
            Assert.That(0d, Is.EqualTo(new Expression("Sin(0)").Evaluate()));
            Assert.That(2d, Is.EqualTo(new Expression("Sqrt(4)").Evaluate()));
            Assert.That(0d, Is.EqualTo(new Expression("Tan(0)").Evaluate()));
            Assert.That(1d, Is.EqualTo(new Expression("Truncate(1.7)").Evaluate()));
        }

        [Test]
        public void ExpressionShouldEvaluateCustomFunctions()
        {
            var e = new Expression("SecretOperation(3, 6)");

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
                {
                    if (name == "SecretOperation")
                        args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
                };

            Assert.That(9, Is.EqualTo(e.Evaluate()));
        }

        [Test]
        public void ExpressionShouldEvaluateCustomFunctionsWithParameters()
        {
            var e = new Expression("SecretOperation([e], 6) + f");
            e.Parameters["e"] = 3;
            e.Parameters["f"] = 1;

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
                {
                    if (name == "SecretOperation")
                        args.Result = (int)args.Parameters[0].Evaluate() + (int)args.Parameters[1].Evaluate();
                };

            Assert.That(10, Is.EqualTo(e.Evaluate()));
        }

        [Test]
		public void ExpressionShouldEvaluateParameters()
		{
			var e = new Expression("Round(Pow(Pi, 2) + Pow([Pi Squared], 2) + [X], 2)");
		    
			e.Parameters["Pi Squared"] = new Expression("Pi * [Pi]");
			e.Parameters["X"] = 10;

			e.EvaluateParameter += delegate(string name, ParameterArgs args)
				{
					if (name == "Pi")
						args.Result = 3.14;
				};

			Assert.That(117.07, Is.EqualTo(e.Evaluate()));
		}

        [Test]
        public void ShouldEvaluateConditionnal()
        {
            var eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
            eif.Parameters["divider"] = 5;
            eif.Parameters["divided"] = 5;

            Assert.That(1d, Is.EqualTo(eif.Evaluate()));

            eif = new Expression("if([divider] <> 0, [divided] / [divider], 0)");
            eif.Parameters["divider"] = 0;
            eif.Parameters["divided"] = 5;
            Assert.That(0, Is.EqualTo(eif.Evaluate()));
        }

        [Test]
        public void ShouldOverrideExistingFunctions()
        {
            var e = new Expression("Round(1.99, 2)");

            Assert.That(1.99d, Is.EqualTo(e.Evaluate()));

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
            {
                if (name == "Round")
                    args.Result = 3;
            };

            Assert.That(3, Is.EqualTo(e.Evaluate()));
        }

        [Test]
        public void ShouldEvaluateInOperator()
        {
            // The last argument should not be evaluated
            var ein = new Expression("in((2 + 2), [1], [2], 1 + 2, 4, 1 / 0)");
            ein.Parameters["1"] = 2;
            ein.Parameters["2"] = 5;

            Assert.That(true, Is.EqualTo(ein.Evaluate()));

            var eout = new Expression("in((2 + 2), [1], [2], 1 + 2, 3)");
            eout.Parameters["1"] = 2;
            eout.Parameters["2"] = 5;

            Assert.That(false, Is.EqualTo(eout.Evaluate()));

            // Should work with strings
            var estring = new Expression("in('to' + 'to', 'titi', 'toto')");

            Assert.That(true, Is.EqualTo(estring.Evaluate()));

        }

        [Test]
        public void ShouldEvaluateOperators()
        {
            var expressions = new Dictionary<string, object>
                                  {
                                      {"!true", false},
                                      {"not false", true},
                                      {"2 * 3", 6},
                                      {"6 / 2", 3d},
                                      {"7 % 2", 1},
                                      {"2 + 3", 5},
                                      {"2 - 1", 1},
                                      {"1 < 2", true},
                                      {"1 > 2", false},
                                      {"1 <= 2", true},
                                      {"1 <= 1", true},
                                      {"1 >= 2", false},
                                      {"1 >= 1", true},
                                      {"1 = 1", true},
                                      {"1 == 1", true},
                                      {"1 != 1", false},
                                      {"1 <> 1", false},
                                      {"1 & 1", 1},
                                      {"1 | 1", 1},
                                      {"1 ^ 1", 0},
                                      {"~1", ~1},
                                      {"2 >> 1", 1},
                                      {"2 << 1", 4},
                                      {"true && false", false},
                                      {"true and false", false},
                                      {"true || false", true},
                                      {"true or false", true},
                                      {"if(true, 0, 1)", 0},
                                      {"if(false, 0, 1)", 1}
                                  };

            foreach (KeyValuePair<string, object> pair in expressions)
            {
                Assert.That(pair.Value, Is.EqualTo(new Expression(pair.Key).Evaluate()));
            }
            
        }

        [Test]
        public void ShouldHandleOperatorsPriority()
        {
            Assert.That(8, Is.EqualTo(new Expression("2+2+2+2").Evaluate()));
            Assert.That(16, Is.EqualTo(new Expression("2*2*2*2").Evaluate()));
            Assert.That(6, Is.EqualTo(new Expression("2*2+2").Evaluate()));
            Assert.That(6, Is.EqualTo(new Expression("2+2*2").Evaluate()));

            Assert.That(9d, Is.EqualTo(new Expression("1 + 2 + 3 * 4 / 2").Evaluate()));
            Assert.That(13.5, Is.EqualTo(new Expression("18/2/2*3").Evaluate()));
        }

        [Test]
        public void ShouldNotLoosePrecision()
        {
            Assert.That(0.5, Is.EqualTo(new Expression("3/6").Evaluate()));
        }

        [Test]
        public void ShouldThrowAnExpcetionWhenInvalidNumber()
        {
            try
            {
                new Expression("4. + 2").Evaluate();
                Assert.Fail("Invalid number was calculated");
            }
            catch (EvaluationException e)
            {
                Console.WriteLine("Error catched: " + e.Message);
            }
        }

        [Test]
        public void ShouldNotRoundDecimalValues()
        {
            Assert.That(false, Is.EqualTo(new Expression("0 <= -0.6").Evaluate()));
        }

        [Test]
        public void ShouldEvaluateTernaryExpression()
        {
            Assert.That(1, Is.EqualTo(new Expression("1+2<3 ? 3+4 : 1").Evaluate()));
        }

        [Test]
        public void ShouldSerializeExpression()
        {
            Assert.That("True and False", Is.EqualTo(new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)).ToString()));
            Assert.That("1 / 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.Div, new ValueExpression(1), new ValueExpression(2)).ToString()));
            Assert.That("1 = 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.Equal, new ValueExpression(1), new ValueExpression(2)).ToString()));
            Assert.That("1 > 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.Greater, new ValueExpression(1), new ValueExpression(2)).ToString()));
            Assert.That("1 >= 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.GreaterOrEqual, new ValueExpression(1), new ValueExpression(2)).ToString()));
            Assert.That("1 < 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.Lesser, new ValueExpression(1), new ValueExpression(2)).ToString()));
            Assert.That("1 <= 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.LesserOrEqual, new ValueExpression(1), new ValueExpression(2)).ToString()));
            Assert.That("1 - 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.Minus, new ValueExpression(1), new ValueExpression(2)).ToString()));
            Assert.That("1 % 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.Modulo, new ValueExpression(1), new ValueExpression(2)).ToString()));
            Assert.That("1 != 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.NotEqual, new ValueExpression(1), new ValueExpression(2)).ToString()));
            Assert.That("True or False", Is.EqualTo(new BinaryExpression(BinaryExpressionType.Or, new ValueExpression(true), new ValueExpression(false)).ToString()));
            Assert.That("1 + 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2)).ToString()));
            Assert.That("1 * 2", Is.EqualTo(new BinaryExpression(BinaryExpressionType.Times, new ValueExpression(1), new ValueExpression(2)).ToString()));

            Assert.That("-(True and False)", Is.EqualTo(new UnaryExpression(UnaryExpressionType.Negate, new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false))).ToString()));
            Assert.That("!(True and False)", Is.EqualTo(new UnaryExpression(UnaryExpressionType.Not, new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false))).ToString()));

            Assert.That("test(True and False, -(True and False))", Is.EqualTo(new Function(new Identifier("test"), new LogicalExpression[] { new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false)), new UnaryExpression(UnaryExpressionType.Negate, new BinaryExpression(BinaryExpressionType.And, new ValueExpression(true), new ValueExpression(false))) }).ToString()));

            Assert.That("True", Is.EqualTo(new ValueExpression(true).ToString()));
            Assert.That("False", Is.EqualTo(new ValueExpression(false).ToString()));
            Assert.That("1", Is.EqualTo(new ValueExpression(1).ToString()));
            Assert.That("1.234", Is.EqualTo(new ValueExpression(1.234).ToString()));
            Assert.That("'hello'", Is.EqualTo(new ValueExpression("hello").ToString()));
            Assert.That("#" + new DateTime(2009, 1, 1) + "#", Is.EqualTo(new ValueExpression(new DateTime(2009, 1, 1)).ToString()));

            Assert.That("Sum(1 + 2)", Is.EqualTo(new Function(new Identifier("Sum"), new [] { new BinaryExpression(BinaryExpressionType.Plus, new ValueExpression(1), new ValueExpression(2))}).ToString()));
        }

        [Test]
        public void ShouldHandleStringConcatenation()
        {
            Assert.That("toto", Is.EqualTo(new Expression("'to' + 'to'").Evaluate()));
            Assert.That("one2", Is.EqualTo(new Expression("'one' + 2").Evaluate()));
            Assert.That(3M, Is.EqualTo(new Expression("1 + '2'").Evaluate()));
        }

        [Test]
        public void ShouldDetectSyntaxErrorsBeforeEvaluation()
        {
            var e = new Expression("a + b * (");
            Assert.Null(e.Error);
            Assert.True(e.HasErrors());
            Assert.True(e.HasErrors());
            Assert.NotNull(e.Error);

            e = new Expression("+ b ");
            Assert.Null(e.Error);
            Assert.True(e.HasErrors());
            Assert.NotNull(e.Error);
        }

        [Test]
        public void ShouldReuseCompiledExpressionsInMultiThreadedMode()
        {
            // Repeats the tests n times
            for (int cpt = 0; cpt < 3; cpt++)
            {
                const int nbthreads = 30;
                _exceptions = new List<Exception>();
                var threads = new Thread[nbthreads];

                // Starts threads
                for (int i = 0; i < nbthreads; i++)
                {
                    var thread = new Thread(WorkerThread);
                    thread.Start();
                    threads[i] = thread;
                }

                // Waits for end of threads
                bool running = true;
                while (running)
                {
                    Thread.Sleep(100);
                    running = false;
                    for (int i = 0; i < nbthreads; i++)
                    {
                        if (threads[i].ThreadState == ThreadState.Running)
                            running = true;
                    }
                }

                if (_exceptions.Count > 0)
                {
                    Console.WriteLine(_exceptions[0].StackTrace);
                    Assert.Fail(_exceptions[0].Message);
                }
            }
        }

        private List<Exception> _exceptions;

        private void WorkerThread()
        {
            try
            {
                var r1 = new Random((int)DateTime.Now.Ticks);
                var r2 = new Random((int)DateTime.Now.Ticks);
                int n1 = r1.Next(10);
                int n2 = r2.Next(10);

                // Constructs a simple addition randomly. Odds are that the same expression gets constructed multiple times by different threads
                var exp = n1 + " + " + n2;
                var e = new Expression(exp);
                Assert.True(e.Evaluate().Equals(n1 + n2));
            }
            catch (Exception e)
            {
                _exceptions.Add(e);
            }
        }

        [Test]
        public void ShouldHandleCaseSensitiveness()
        {
            Assert.That(1M, Is.EqualTo(new Expression("aBs(-1)", EvaluateOptions.IgnoreCase).Evaluate()));
            Assert.That(1M, Is.EqualTo(new Expression("Abs(-1)", EvaluateOptions.None).Evaluate()));

            try
            {
                Assert.That(1M, Is.EqualTo(new Expression("aBs(-1)", EvaluateOptions.None).Evaluate()));
            }
            catch (ArgumentException)
            {
                return;
            }
            catch (Exception)
            {
                Assert.Fail("Unexpected exception");
            }

            Assert.Fail("Should throw ArgumentException");
        }

        [Test]
        public void ShouldHandleCustomParametersWhenNoSpecificParameterIsDefined()
        {
            var e = new Expression("Round(Pow([Pi], 2) + Pow([Pi], 2) + 10, 2)");

            e.EvaluateParameter += delegate(string name, ParameterArgs arg)
            {
                if (name == "Pi")
                    arg.Result = 3.14;
            };

            e.Evaluate();
        }

        [Test]
        public void ShouldHandleCustomFunctionsInFunctions()
        {
            var e = new Expression("if(true, func1(x) + func2(func3(y)), 0)");

            e.EvaluateFunction += delegate(string name, FunctionArgs arg)
            {
                switch (name)
                {
                    case "func1": arg.Result = 1;
                        break;
                    case "func2": arg.Result = 2 * Convert.ToDouble(arg.Parameters[0].Evaluate());
                        break;
                    case "func3": arg.Result = 3 * Convert.ToDouble(arg.Parameters[0].Evaluate());
                        break;
                }
            };

            e.EvaluateParameter += delegate(string name, ParameterArgs arg)
            {
                switch (name)
                {
                    case "x": arg.Result = 1;
                        break;
                    case "y": arg.Result = 2;
                        break;
                    case "z": arg.Result = 3;
                        break;
                }
            };

            Assert.That(13d, Is.EqualTo(e.Evaluate()));
        }


        [Test]
        public void ShouldParseScientificNotation()
        {
            Assert.That(12.2d, Is.EqualTo(new Expression("1.22e1").Evaluate()));
            Assert.That(100d, Is.EqualTo(new Expression("1e2").Evaluate()));
            Assert.That(100d, Is.EqualTo(new Expression("1e+2").Evaluate()));
            Assert.That(0.01d, Is.EqualTo(new Expression("1e-2").Evaluate()));
            Assert.That(0.001d, Is.EqualTo(new Expression(".1e-2").Evaluate()));
            Assert.That(10000000000d, Is.EqualTo(new Expression("1e10").Evaluate()));
        }

        [Test]
        public void ShouldEvaluateArrayParameters()
        {
            var e = new Expression("x * x", EvaluateOptions.IterateParameters);
            e.Parameters["x"] = new [] { 0, 1, 2, 3, 4 };

            var result = (IList)e.Evaluate();

            Assert.That(0, Is.EqualTo(result[0]));
            Assert.That(1, Is.EqualTo(result[1]));
            Assert.That(4, Is.EqualTo(result[2]));
            Assert.That(9, Is.EqualTo(result[3]));
            Assert.That(16, Is.EqualTo(result[4]));
        }

        [Test]
        public void CustomFunctionShouldReturnNull()
        {
            var e = new Expression("SecretOperation(3, 6)");

            e.EvaluateFunction += delegate(string name, FunctionArgs args)
            {
                Assert.False(args.HasResult);
                if (name == "SecretOperation")
                    args.Result = null;
                Assert.True(args.HasResult);
            };

            Assert.That(null, Is.EqualTo(e.Evaluate()));
        }

        [Test]
        public void CustomParametersShouldReturnNull()
        {
            var e = new Expression("x");

            e.EvaluateParameter += delegate(string name, ParameterArgs args)
            {
                Assert.False(args.HasResult);
                if (name == "x")
                    args.Result = null;
                Assert.True(args.HasResult);
            };

            Assert.That(null, Is.EqualTo(e.Evaluate()));
        }

        [Test]
        public void ShouldCompareDates()
        {
            Assert.That(true, Is.EqualTo(new Expression("#1/1/2009#==#1/1/2009#").Evaluate()));
            Assert.That(false, Is.EqualTo(new Expression("#2/1/2009#==#1/1/2009#").Evaluate()));
        }

        [Test]
        public void ShouldRoundAwayFromZero()
        {
            Assert.That(22d, Is.EqualTo(new Expression("Round(22.5, 0)").Evaluate()));
            Assert.That(23d, Is.EqualTo(new Expression("Round(22.5, 0)", EvaluateOptions.RoundAwayFromZero).Evaluate()));
        }

        [Test]
        public void ShouldEvaluateSubExpressions()
        {
            var volume = new Expression("[surface] * h");
            var surface = new Expression("[l] * [L]");
            volume.Parameters["surface"] = surface;
            volume.Parameters["h"] = 3;
            surface.Parameters["l"] = 1;
            surface.Parameters["L"] = 2;

            Assert.That(6, Is.EqualTo(volume.Evaluate()));
        }

        [Test]
        public void ShouldHandleLongValues()
        {
            Assert.That(40000000000 + 1f, Is.EqualTo(new Expression("40000000000+1").Evaluate()));
        }

        [Test]
        public void ShouldCompareLongValues()
        {
            Assert.That(false, Is.EqualTo(new Expression("(0=1500000)||(((0+2200000000)-1500000)<0)").Evaluate()));
        }

        [Test, ExpectedException(typeof(InvalidOperationException))]
        public void ShouldDisplayErrorIfUncompatibleTypes()
        {
            var e = new Expression("(a > b) + 10");
            e.Parameters["a"] = 1;
            e.Parameters["b"] = 2;
            e.Evaluate();
        }

        [Test]
        public void ShouldNotConvertRealTypes() 
        {
            var e = new Expression("x/2");
            e.Parameters["x"] = 2F;
            Assert.That(e.Evaluate(), Is.TypeOf(typeof(float)));

            e = new Expression("x/2");
            e.Parameters["x"] = 2D;
            Assert.That(e.Evaluate(), Is.TypeOf(typeof(double)));

            e = new Expression("x/2");
            e.Parameters["x"] = 2m;
			Assert.That(e.Evaluate(), Is.TypeOf(typeof(decimal)));

            e = new Expression("a / b * 100");
            e.Parameters["a"] = 20M;
            e.Parameters["b"] = 20M;
            Assert.That(100M, Is.EqualTo(e.Evaluate()));

        }

        [Test]
        public void ShouldShortCircuitBooleanExpressions()
        {
            var e = new Expression("([a] != 0) && ([b]/[a]>2)");
            e.Parameters["a"] = 0;

            Assert.That(false, Is.EqualTo(e.Evaluate()));
        }
    }
}

