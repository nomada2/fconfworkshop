﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataParallelism.CSharp;

namespace DataParallelism.CSharp
{
    using static Functional.Functional;

    // TASK : implement a parallel Reducer function
    public static class ParalellMapReduce
    {
        //  A parallel Reduce function implementation using Aggregate
        public static TValue Reduce<TValue>(this ParallelQuery<TValue> source, Func<TValue, TValue, TValue> func) => 
           
            ParallelEnumerable.Aggregate(source,  
                        (item1, item2) => func(item1, item2));  


        public static TResult Reduce<TValue, TResult>(this IEnumerable<TValue> source, TResult seed, Func<TResult, TValue, TResult> reduce) =>
            source.AsParallel()
            .Aggregate(seed, (local, value) => reduce(local, value),
            (overall, local) => reduce(overall, local), overall => overall);
        
        public static Func<Func<TSource, TSource, TSource>, TSource> Reduce<TSource>(this IEnumerable<TSource> source)
            => func => source.AsParallel().Aggregate((item1, item2) => func(item1, item2));

        public static IEnumerable<IGrouping<TKey, TMapped>> Map<TSource, TKey, TMapped>(this IList<TSource> source, Func<TSource, IEnumerable<TMapped>> map, Func<TMapped, TKey> keySelector) =>
                    source.AsParallel()
              .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                   .WithDegreeOfParallelism(Environment.ProcessorCount)
                   .SelectMany(map)
                   .GroupBy(keySelector)
                   .ToList();
        
        public static TResult[] Reduce<TSource, TKey, TMapped, TResult>(this IEnumerable<IGrouping<TKey, TMapped>> source, Func<IGrouping<TKey, TMapped>, TResult> reduce) => 
                      source.AsParallel()
              .WithExecutionMode(ParallelExecutionMode.ForceParallelism)
                     .WithDegreeOfParallelism(Environment.ProcessorCount)
                     .Select(reduce).ToArray();

        public static Func<Func<TSource, IEnumerable<TMapped>>, Func<TMapped, TKey>, Func<IGrouping<TKey, TMapped>, TResult>, TResult[]> MapReduceFunc<TSource, TMapped, TKey, TResult>(
         this IList<TSource> source)
        {
            Func<IList<TSource>, Func<TSource, IEnumerable<TMapped>>, Func<TMapped, TKey>, IEnumerable<IGrouping<TKey, TMapped>>> mapFunc = Map;
            Func<IEnumerable<IGrouping<TKey, TMapped>>, Func<IGrouping<TKey, TMapped>, TResult>, TResult[]> reduceFunc = Reduce<TSource, TKey, TMapped, TResult>;
            
            return mapFunc.Compose(reduceFunc).Partial(source);
        }
    }
}