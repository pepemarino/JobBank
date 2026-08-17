namespace JobBank.Util
{
    using System.Text.Json;

    /// <summary>
    /// use this class to track changes to an object by 
    /// comparing its current state to an initial snapshot taken at the time of instantiation.
    /// Do not use this class for tracking changes to complex objects with nested properties, 
    /// as it performs a shallow comparison based on JSON serialization and it 
    /// may not accurately detect changes in nested objects or collections. 
    /// It is best suited for simple objects with primitive properties.
    /// Furthermore, for complex ojecgts it could be a performance concern to serialize 
    /// the entire object graph on each change check, so use this class judiciously 
    /// in performance-sensitive scenarios.
    /// 
    /// As Bugs Bunny says: "You'll be sorry!"
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ChangeTracker<T> where T : class
    {
        private readonly string _initialSnapshot;

        /// <summary>
        /// Gets the current object being tracked.
        /// This is the oject that we modify and save.
        /// </summary>
        public T Current { get; }

        public ChangeTracker(T original)
        {
            // Clone the original object so 'Current' is a separate instance
            var json = JsonSerializer.Serialize(original);
            _initialSnapshot = json;
            Current = JsonSerializer.Deserialize<T>(json)!;
        }
        
        public bool HasChanged()
        {
            var currentSnapshot = JsonSerializer.Serialize(Current);
            return _initialSnapshot != currentSnapshot;
        }
    }
}
