using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ResultApp.Functional.Result;
using System.IO;

namespace ResultApp
{
    namespace Functional.Result
    {
        using static F;

        public static partial class F
        {
            public static Result<T, Exception> TryCatch<T>(Func<T> func)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {

                    return ex;
                }
            }
            public static Result.Error<L> Error<L>(L l) => new Result.Error<L>(l);
            public static Result.Ok<R> Ok<R>(R r) => new Result.Ok<R>(r);
        }

        public struct Result<L, R>
        {
            public L Error { get; }
            public R Ok { get; }

            public bool IsValid { get; }
            public bool HasError => !IsValid;

            internal Result(L left)
            {
                IsValid = false;
                Error = left;
                Ok = default(R);
            }

            internal Result(R right)
            {
                IsValid = true;
                Ok = right;
                Error = default(L);
            }

            public static implicit operator Result<L, R>(L left) => new Result<L, R>(left);
            public static implicit operator Result<L, R>(R right) => new Result<L, R>(right);

            public static implicit operator Result<L, R>(Result.Error<L> left) => new Result<L, R>(left.Value);
            public static implicit operator Result<L, R>(Result.Ok<R> right) => new Result<L, R>(right.Value);

            public TR Match<TR>(Func<L, TR> Left, Func<R, TR> Right)
               => HasError ? Left(this.Error) : Right(this.Ok);

        }

        public static class Result
        {
            public struct Error<L>
            {
                internal L Value { get; }
                internal Error(L value) { Value = value; }

                public override string ToString() => $"Left({Value})";
            }

            public struct Ok<R>
            {
                internal R Value { get; }
                internal Ok(R value) { Value = value; }

                public override string ToString() => $"Right({Value})";

                public Ok<RR> Map<L, RR>(Func<R, RR> f) => Ok(f(Value));
                public Result<L, RR> Bind<L, RR>(Func<R, Result<L, RR>> f) => f(Value);
            }
        }

        public static class ResultExt
        {
            public static Result<L, RR> Map<L, R, RR>
               (this Result<L, R> @this, Func<R, RR> f)
               => @this.Match<Result<L, RR>>(
                  l => Error(l),
                  r => Ok(f(r)));

            public static Result<LL, RR> Map<L, LL, R, RR>
               (this Result<L, R> @this, Func<L, LL> Ok, Func<R, RR> Failure)
               => @this.Match<Result<LL, RR>>(
                  l => Error(Ok(l)),
                  r => F.Ok(Failure(r)));


            public static Result<L, RR> Bind<L, R, RR>
               (this Result<L, R> @this, Func<R, Result<L, RR>> f)
               => @this.Match(
                  l => Error(l),
                  r => f(r));

            public static Result<L, R> Select<L, T, R>(this Result<L, T> @this
               , Func<T, R> map) => @this.Map(map);


            public static Result<L, RR> SelectMany<L, T, R, RR>(this Result<L, T> @this
               , Func<T, Result<L, R>> bind, Func<T, R, RR> project)
               => @this.Match(
                  Left: l => Error(l),
                  Right: t =>
                     bind(@this.Ok).Match<Result<L, RR>>(
                        Left: l => Error(l),
                        Right: r => project(t, r)));
        }
    }

    namespace Functional.Exceptional
    {
        public static partial class F
        {
            public static Exceptional<T> Exceptional<T>(T value) => new Exceptional<T>(value);
        }

        public struct Exceptional<T>
        {
            internal Exception Ex { get; }
            internal T Value { get; }

            public bool Success => Ex == null;
            public bool Exception => Ex != null;

            internal Exceptional(Exception ex)
            {
                if (ex == null) throw new ArgumentNullException(nameof(ex));
                Ex = ex;
                Value = default(T);
            }

            internal Exceptional(T right)
            {
                Value = right;
                Ex = null;
            }

            public static implicit operator Exceptional<T>(Exception left) => new Exceptional<T>(left);
            public static implicit operator Exceptional<T>(T right) => new Exceptional<T>(right);

            public TR Match<TR>(Func<Exception, TR> Exception, Func<T, TR> Success)
               => this.Exception ? Exception(Ex) : Success(Value);
        }

        public static class Exceptional
        {
            public static Func<T, Exceptional<T>> Return<T>()
               => t => t;

            public static Exceptional<R> Of<R>(Exception left)
               => new Exceptional<R>(left);

            public static Exceptional<R> Of<R>(R right)
               => new Exceptional<R>(right);

            public static Exceptional<R> Apply<T, R>
               (this Exceptional<Func<T, R>> @this, Exceptional<T> arg)
               => @this.Match(
                  Exception: ex => ex,
                  Success: func => arg.Match(
                     Exception: ex => ex,
                     Success: t => new Exceptional<R>(func(t))));


            public static Exceptional<RR> Map<R, RR>(this Exceptional<R> @this
               , Func<R, RR> func) => @this.Success ? func(@this.Value) : new Exceptional<RR>(@this.Ex);

            public static Exceptional<RR> Bind<R, RR>(this Exceptional<R> @this
               , Func<R, Exceptional<RR>> func)
                => @this.Success ? func(@this.Value) : new Exceptional<RR>(@this.Ex);


            public static Exceptional<R> Select<T, R>(this Exceptional<T> @this
               , Func<T, R> map) => @this.Map(map);

            public static Exceptional<RR> SelectMany<T, R, RR>(this Exceptional<T> @this
               , Func<T, Exceptional<R>> bind, Func<T, R, RR> project)
            {
                if (@this.Exception) return new Exceptional<RR>(@this.Ex);
                var bound = bind(@this.Value);
                return bound.Exception
                   ? new Exceptional<RR>(bound.Ex)
                   : project(@this.Value, bound.Value);
            }
        }

    }

    ;
    class Program
    {
        static void Main(string[] args)
        {
            string path = "";

            Result<int, string> x = "";



            var s = F.TryCatch(() => File.OpenRead(path))
                 .Map(
                     Ok: stream =>
                     {
                         var bytes = new byte[(int)stream.Length];
                         stream.Read(bytes, 0, bytes.Length);
                         return bytes;
                     },
                     Failure: ex =>
                         {
                             Console.WriteLine($"Log - {ex.Message}");
                             return ex;
                         }).Match(
                         Left: bytes => bytes.Length,
                         Right: ex => { Console.WriteLine(ex.Message); return 0; });

        }
    }
}
