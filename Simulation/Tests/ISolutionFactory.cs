using Sheet.Simulation.Core;

namespace Sheet.Simulation.Tests
{
    public interface ISolutionFactory
    {
        AndGate CreateAndGate(Context context, double x, double y, string name);
        BufferGate CreateBufferGate(Context context, double x, double y, string name);
        Element CreateElementFromType(Context context, string type, double x, double y);
        MemoryResetPriority CreateMemoryResetPriority(Context context, double x, double y, string name);
        MemorySetPriority CreateMemorySetPriority(Context context, double x, double y, string name);
        NandGate CreateNandGate(Context context, double x, double y, string name);
        NorGate CreateNorGate(Context context, double x, double y, string name);
        NotGate CreateNotGate(Context context, double x, double y, string name);
        OrGate CreateOrGate(Context context, double x, double y, string name);
        Pin CreatePin(Context context, double x, double y, Element parent, string name, string factoryName, PinType type, bool isPinTypeUndefined);
        Signal CreateSignal(Context context, double x, double y, string name);
        Tag CreateSignalTag(string designation, string description, string signal, string condition);
        TimerOff CreateTimerOff(Context context, double x, double y, string name);
        TimerOn CreateTimerOn(Context context, double x, double y, string name);
        TimerPulse CreateTimerPulse(Context context, double x, double y, string name);
        Wire CreateWire(Context context, Pin start, Pin end);
        XnorGate CreateXnorGate(Context context, double x, double y, string name);
        XorGate CreateXorGate(Context context, double x, double y, string name);
    }
}