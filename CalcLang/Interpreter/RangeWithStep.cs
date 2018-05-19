using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcLang.Interpreter
{
    public struct RangeWithStep : IEnumerable<long>
    {
        public long Start { get; private set; }
        public long End { get; private set; }
        public long Step { get; private set; }
        public bool Inclusive { get; private set; }

        public long Length => this.LongCount();

        public long this[long index]
        {
            get
            {
                return Start + index * Step;
            }
        }

        public RangeWithStep(long start, long end, long step, bool inclusive)
        {
            Start = start;
            End = end;
            Inclusive = inclusive;
            Step = Math.Sign(end - start) * step;
        }

        public IEnumerator<long> GetEnumerator()
        {
            return new StructRangeWithStepEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override bool Equals(object obj)
        {
            return obj != null && obj is RangeWithStep && Equals((RangeWithStep)obj);
        }

        public bool Equals(RangeWithStep other)
        {
            return other.Start == Start && other.End == End && other.Step == Step;
        }

        public override int GetHashCode()
        {
            return (int)((((Start * 13) ^ End) * 13) ^ Step);
        }

        public override string ToString()
        {
            return $"[{Start}{(Inclusive ? "..." : "..")}{End}:{Step}]";
        }

        public static implicit operator RangeWithStep(Range range)
            => new RangeWithStep(range.Start, range.End, Math.Sign(range.End - range.Start), range.Inclusive);

        public class StructRangeWithStepEnumerator : IEnumerator<long>
        {
            public long Current { get; private set; }

            object IEnumerator.Current => Current;

            private RangeWithStep range;

            private Func<long, long, bool> compare;

            public StructRangeWithStepEnumerator(RangeWithStep range)
            {
                this.range = range;
                Current = range.Start - range.Step;

                if (range.Inclusive)
                {
                    if (range.Step > 0)
                        compare = (a, b) => a <= b;
                    else
                        compare = (a, b) => a >= b;
                }
                else
                {
                    if (range.Step > 0)
                        compare = (a, b) => a < b;
                    else
                        compare = (a, b) => a > b;
                }
            }

            public void Dispose()
            {
            }

            public bool MoveNext() => compare((Current = Current + range.Step), range.End);

            public void Reset()
            {
                Current = range.Start - range.Step;
            }
        }
    }
}
