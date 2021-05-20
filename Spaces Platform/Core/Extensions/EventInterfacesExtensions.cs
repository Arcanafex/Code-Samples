namespace UnityEngine.EventSystems
{
    public interface IGazeEnterHandler : IEventSystemHandler
    {
        void OnGazeEnter(PointerEventData eventData);
    }

    public interface IGazeStayHandler : IEventSystemHandler
    {
        void OnGazeStay(PointerEventData eventData);
    }

    public interface IGazeExitHandler : IEventSystemHandler
    {
        void OnGazeExit(PointerEventData eventData);
    }

    public static class ExecuteEventsExtensions
    {
        private static readonly UnityEngine.EventSystems.ExecuteEvents.EventFunction<IGazeEnterHandler> s_GazeEnterHandler = Execute;

        private static void Execute(IGazeEnterHandler handler, BaseEventData eventData)
        {
            handler.OnGazeEnter(UnityEngine.EventSystems.ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
        }

        public static UnityEngine.EventSystems.ExecuteEvents.EventFunction<IGazeEnterHandler> gazeEnterHandler
        {
            get { return s_GazeEnterHandler; }
        }


        private static readonly UnityEngine.EventSystems.ExecuteEvents.EventFunction<IGazeStayHandler> s_GazeStayHandler = Execute;

        private static void Execute(IGazeStayHandler handler, BaseEventData eventData)
        {
            handler.OnGazeStay(UnityEngine.EventSystems.ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
        }

        public static UnityEngine.EventSystems.ExecuteEvents.EventFunction<IGazeStayHandler> gazeStayHandler
        {
            get { return s_GazeStayHandler; }
        }


        private static readonly UnityEngine.EventSystems.ExecuteEvents.EventFunction<IGazeExitHandler> s_GazeExitHandler = Execute;

        private static void Execute(IGazeExitHandler handler, BaseEventData eventData)
        {
            handler.OnGazeExit(UnityEngine.EventSystems.ExecuteEvents.ValidateEventData<PointerEventData>(eventData));
        }

        public static UnityEngine.EventSystems.ExecuteEvents.EventFunction<IGazeExitHandler> gazeExitHandler
        {
            get { return s_GazeExitHandler; }
        }

    }
}
