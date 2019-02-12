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
        public bool Inclusive { get; private set; }

        public long Length => Math.Abs(End - Start) + (Inclusive ? 1 : 0);

        private long direction;

        public long this[long index]
        {
            get
            {
                return Start + direction * index;
            }
        }

        public Range(long start, long end, bool inclusive)
        {
            Start = start;
            End = end;
            Inclusive = inclusive;
            direction = Math.Sign(end - start);
        }

        public IEnumerator<long> GetEnumerator() => new StructRangeEnumerator(this);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public RangeWithStep Step(long step)
        {
            return new RangeWithStep(Start, End, direction * step, Inclusive);
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
            return $"[{Start}{(Inclusive ? "..." : "..")}{End}]";
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
            private Func<long, long, bool> compare;

            internal StructRangeEnumerator(Range range)
            {
                this.range = range;
                Current = range.Start - range.direction * 1;

                if (range.Inclusive)
                {
                    if (range.direction > 0)
                        compare = (a, b) => a <= b;
                    if (range.direction == 0)
                        compare = (a, b) => {
                            compare = (aa, bb) => false;
                            return true;
                        };
                    else
                        compare = (a, b) => a >= b;
                }
                else
                {
                    if (range.direction > 0)
                        compare = (a, b) => a < b;
                    if (range.direction == 0)
                        compare = (a, b) => false;
                    else
                        compare = (a, b) => a > b;
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext() => compare((Current += range.direction), range.End);

            public void Reset()
            {
                Current = range.Start - range.direction * 1;
            }
        }
    }
}
