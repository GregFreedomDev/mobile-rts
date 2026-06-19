namespace hvo.Scripts.Utils
{
    /// <summary>
    /// Global gate shared between the unit-drag system and the camera pan controllers.
    /// While a unit is being dragged from the panel onto the grid, camera panning must be
    /// suppressed so the same mouse/touch drag doesn't slide the battlefield under the cursor.
    /// </summary>
    public static class DragState
    {
        public static bool IsDraggingUnit;
    }
}
