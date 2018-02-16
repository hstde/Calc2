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

        private long direction;

        public long this[long index]
        {
            get
            {
                return Start + direction * index;
            }
        }

        public Range(long start, long end)
        {
            Start = start;
            End = end;
            direction = Math.Sign(end - start);
        }

        public IEnumerator<long> GetEnumerator() => new StructRangeEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RangeWithStep Step(long step)
        {
            return new RangeWithStep(Start, End, direction * step);
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
                Current = range.Start - range.direction * 1;
            }

            public void Dispose()
            {
            }

            public bool MoveNext() => Compare((Current += range.direction), range.End);

            private bool Compare(long current, long end)
            {
                if (range.direction > 0)
                    return current < end;
                else
                    return current > end;
            }

            public void Reset()
            {
                Current = range.Start - 1;
            }
        }
    }
}
