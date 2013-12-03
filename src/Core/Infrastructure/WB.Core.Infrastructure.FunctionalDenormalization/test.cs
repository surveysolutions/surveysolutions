using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace crap
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;



    public class PartialAppPlayground
    {
        private static Dispatcher<Command, Nothing> _dispatcher;

        public static void Initialize()
        {
            _dispatcher = new Dispatcher<Command, Nothing>();
            _dispatcher.Register<DoBar>(message => CommandHandlers.Bar(() => new SqlConnection(), message));
            _dispatcher.Register<DoFoo>(message => CommandHandlers.Foo(new SqlConnection(), message));
        }

        public static void Main()
        {
            Initialize();
            _dispatcher.Dispatch(new DoBar());
            _dispatcher.Dispatch(new DoFoo());
        }
    }

    public class Dispatcher<TMessage, TResult>
    {
        private readonly Dictionary<Type, Func<TMessage, TResult>> _dictionary = new Dictionary<Type, Func<TMessage, TResult>>();

        public void Register<T>(Func<T, TResult> func) where T : TMessage
        {
            _dictionary.Add(typeof(T), x => func((T)x));
        }

        public TResult Dispatch(TMessage m)
        {
            Func<TMessage, TResult> handler;
            if (!_dictionary.TryGetValue(m.GetType(), out handler))
            {
                throw new Exception("cannot map " + m.GetType());
            }
            return handler(m);
        }
    }

    public class Nothing
    {
        private Nothing() { }
        public static readonly Nothing Value = new Nothing();
        public override string ToString()
        {
            return "Nothing";
        }
    }

    public interface Command : Message { }
    public interface Message { }

    public class DoFoo : Command
    {
        public string Something { get; set; }
    }
    public class DoBar : Command
    {
        public string Something { get; set; }
    }
    public static class CommandHandlers
    {
        public static Nothing Bar(Func<IDbConnection> connection, DoBar command)
        {
            Console.WriteLine("bar");
            return Nothing.Value;
        }
        public static Nothing Foo(IDbConnection connection, DoFoo command)
        {
            Console.WriteLine("foo");
            return Nothing.Value;
        }
    }

}