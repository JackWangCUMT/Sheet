using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Simulation.Binary
{
    /*
    command Id: sizeof(UInt16)
    command data: sizeof(command)
    ...
    elements counter: sizeof(UInt32)
    
    where:
    Id Command
    ---------------
    0    Solution
    1    Project
    2    Context
    3    Pin
    4    Signal
    5    AndGate
    6    OrGate
    7    TimerOn
    8    TimerOff
    9    TimerPulse
    10   Connect

    Solution command:
    UInt32 Id;

    Project command:
    UInt32 Id;

    Context command:
    UInt32 Id;

    Pin command:
    UInt32 Id;

    Signal command:
    UInt32 Id;
    UInt32 InputPinId;
    UInt32 OutputPinId;

    AndGate command:
    UInt32 Id;
    UInt32 LeftPinId;
    UInt32 RightPinId;
    UInt32 TopPinId;
    UInt32 BottomPinId;

    OrGate command:
    UInt32 Id;
    UInt32 LeftPinId;
    UInt32 RightPinId;
    UInt32 TopPinId;
    UInt32 BottomPinId;

    TimerOn command:
    UInt32 Id;
    UInt32 LeftPinId;
    UInt32 RightPinId;
    UInt32 TopPinId;
    UInt32 BottomPinId;
    Single Delay;

    TimerOff command:
    UInt32 Id;
    UInt32 LeftPinId;
    UInt32 RightPinId;
    UInt32 TopPinId;
    UInt32 BottomPinId;
    Single Delay;

    TimerPulse command:
    UInt32 Id;
    UInt32 LeftPinId;
    UInt32 RightPinId;
    UInt32 TopPinId;
    UInt32 BottomPinId;
    Single Delay;

    Connect command:
    UInt32 Id;
    UInt32 SrcPinId;
    UInt32 DstPinId;
    Byte InvertStart;
    Byte InvertEnd;
    */

    public static class BinaryCommandId
    {
        public const UInt16 Solution = 0;
        public const UInt16 Project = 1;
        public const UInt16 Context = 2;
        public const UInt16 Pin = 3;
        public const UInt16 Signal = 4;
        public const UInt16 AndGate = 5;
        public const UInt16 OrGate = 6;
        public const UInt16 TimerOn = 7;
        public const UInt16 TimerOff = 8;
        public const UInt16 TimerPulse = 9;
        public const UInt16 Connect = 10;
    }
}
