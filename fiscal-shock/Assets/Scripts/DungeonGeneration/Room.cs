namespace FiscalShock {
    namespace Dungeons {
        /// <summary>
        /// Utility class for defining a room based on its center point (within a grid cell).
        /// </summary>
        public class Room {
            // Adjustable center point.
            public float centerPoint { get; set; }
            // Read only width/height.
            public float width { get; }
            public float height { get; }

            public Room(float rWidth, float rHeight) {
                width = rWidth;
                height = rHeight;
            }
        }
    }
}