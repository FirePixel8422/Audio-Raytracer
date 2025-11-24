

public class SimpleReverb
{
    // Comb filters
    private readonly float[][] combBuffers;
    private readonly int[] combIndices;
    private readonly float[] combFeedback;

    private const int CombBufferCount = 4;
    private const float One_Div_CombBufferCount = 1f / CombBufferCount;

    // Allpass filters
    private readonly float[][] allpassBuffers;
    private readonly int[] allpassIndices;
    private readonly float[] allpassGain;

    private const int AllPassBufferCount = 3;


    public SimpleReverb(float sampleRate, float decayFactor = 0.805f, float allpassGainValue = 0.7f)
    {
        int[] combDelayMs = new int[] { 29, 37, 41, 43 };
        int[] allpassDelayMs = new int[] { 12, 24, 36 };

        // Init combs
        combBuffers = new float[CombBufferCount][];
        combIndices = new int[CombBufferCount];
        combFeedback = new float[CombBufferCount];

        for (int i = 0; i < CombBufferCount; i++)
        {
            int sampleCount = (int)(combDelayMs[i] * 0.001f * sampleRate);
            combBuffers[i] = new float[sampleCount];
            combIndices[i] = 0;
            combFeedback[i] = decayFactor; // typical decay factor
        }

        // Init allpass
        allpassBuffers = new float[AllPassBufferCount][];
        allpassIndices = new int[AllPassBufferCount];
        allpassGain = new float[AllPassBufferCount];

        for (int i = 0; i < AllPassBufferCount; i++)
        {
            int sampleCount = (int)(allpassDelayMs[i] * 0.001f * sampleRate);
            allpassBuffers[i] = new float[sampleCount];
            allpassIndices[i] = 0;
            allpassGain[i] = allpassGainValue;
        }
    }

    public float Process(float input)
    {
        float combSum = 0f;

        // Comb filters in parallel
        for (int i = 0; i < CombBufferCount; i++)
        {
            float[] buf = combBuffers[i];
            int idx = combIndices[i];

            float y = buf[idx];
            float val = input + y * combFeedback[i];
            buf[idx] = val;

            idx += 1;
            if (idx >= buf.Length) idx = 0;
            combIndices[i] = idx;
            combSum += y;
        }

        float outSample = combSum * One_Div_CombBufferCount;

        // Allpass filters in series
        for (int i = 0; i < AllPassBufferCount; i++)
        {
            float[] buf = allpassBuffers[i];
            int idx = allpassIndices[i];
            float g = allpassGain[i];

            float bufOut = buf[idx];
            float v = outSample + bufOut * -g;
            buf[idx] = v;
            outSample = bufOut + v * g;

            idx += 1;
            if (idx >= buf.Length) idx = 0;
            allpassIndices[i] = idx;
        }

        return outSample;
    }
}
