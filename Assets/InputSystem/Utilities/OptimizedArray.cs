using System;
using System.Collections.Generic;

namespace ISX
{
    // Helper to avoid array allocations if there's only a single value in the
    // array.
    internal struct OptimizedArray<TValue>
    {
        // We inline the first value so if there's only one, there's
        // no additional allocation. If more are added, we allocate an array.
        public TValue firstValue;
        public TValue[] additionalValues;

        public void Append(TValue value)
        {
            if (firstValue == null)
            {
                firstValue = value;
            }
            else if (additionalValues == null)
            {
                additionalValues = new TValue[1];
                additionalValues[0] = value;
            }
            else
            {
                var numAdditionalProcessors = additionalValues.Length;
                Array.Resize(ref additionalValues, numAdditionalProcessors + 1);
                additionalValues[numAdditionalProcessors] = value;
            }
        }

        public void Remove(TValue value)
        {
            if (EqualityComparer<TValue>.Default.Equals(firstValue, value))
            {
                if (additionalValues != null)
                {
                    firstValue = additionalValues[0];
                    if (additionalValues.Length == 1)
                        additionalValues = null;
                    else
                    {
                        Array.Copy(additionalValues, 1, additionalValues, 0, additionalValues.Length - 1);
                        Array.Resize(ref additionalValues, additionalValues.Length - 1);
                    }
                }
                else
                {
                    firstValue = default(TValue);
                }
            }
            else if (additionalValues != null)
            {
                var numAdditionalProcessors = additionalValues.Length;
                for (var i = 0; i < numAdditionalProcessors; ++i)
                {
                    if (EqualityComparer<TValue>.Default.Equals(additionalValues[i], value))
                    {
                        if (i == numAdditionalProcessors - 1)
                        {
                            Array.Resize(ref additionalValues, numAdditionalProcessors - 1);
                        }
                        else
                        {
                            var newAdditionalProcessors = new IInputProcessor<TValue>[numAdditionalProcessors - 1];
                            if (i > 0)
                                Array.Copy(additionalValues, 0, newAdditionalProcessors, 0, i);
                            Array.Copy(additionalValues, i + 1, newAdditionalProcessors, i,
                                numAdditionalProcessors - i);
                        }
                        break;
                    }
                }
            }
        }
    }
}
