using System;
using Leap;

namespace WpfApplication1
{
    class MyLeapListener : Listener
    {
        public event EventHandler OnFrameEvent = null;

        public override void OnFrame(Controller ctl)
        {
            if (OnFrameEvent != null)
            {
                OnFrameEvent.Invoke(ctl, null);
            }
        }
        public override void OnConnect(Controller controller)
        {
            controller.EnableGesture(Gesture.GestureType.TYPE_CIRCLE);
            controller.EnableGesture(Gesture.GestureType.TYPE_KEY_TAP);
            controller.EnableGesture(Gesture.GestureType.TYPE_SCREEN_TAP);
            controller.EnableGesture(Gesture.GestureType.TYPE_SWIPE);
        }
    }
}
