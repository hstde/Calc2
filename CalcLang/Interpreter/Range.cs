using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public struct Range : IEnumerable<long>
    {
        public long Start { get; private set; }
        public long End { get; private set; }

        public long this[long index]
        {
            get
            {
                return Start + index;
            }
        }

        public Range(long start, long end)
        {
            Start = start;
            End = end;
        }

        public IEnumerator<long> GetEnumerator() => new StructRangeEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RangeWithStep Step(long step)
        {
            return new RangeWithStep(Start, End, step);
        }

        public override bool Equals(object obj)
        {
            return obj != null && obj is Range && Equals((Range)obj);
        }

        public bool Equals(Range other)
        {
            return other.Start == Start && other.End == End;
        }

        public override string ToString()
        {
            return $"[{Start}..{End}]";
        }

        public override int GetHashCode()
        {
            return (int)((Start * 13) ^ End);
        }

        public class StructRangeEnumerator : IEnumerator<long>
        {
            public long Current { get; private set; }

            object IEnumerator.Current => Current;

            private Range range;

            internal StructRangeEnumerator(Range range)
            {
                this.range = range;
                Current = range.Start - 1;
            }

            public void Dispose()
            {
            }

            public bool MoveNext() => ++Current < range.End;

            public void Reset()
            {
                Current = range.Start - 1;
            }
        }
    }
}
