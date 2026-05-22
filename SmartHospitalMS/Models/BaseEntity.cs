using System;

namespace SmartHospitalMS
{
    /// <summary>
    /// BaseEntity is the "Parent" class.
    /// JS Analogy: Like a base class in ES6.
    /// Every model will inherit ID and CreatedAt from here.
    /// </summary>
    public abstract class BaseEntity
    {
        // Encapsulation: private field with public property
        private DateTime _createdAt;

        public int ID { get; set; }
        
        public DateTime CreatedAt 
        { 
            get => _createdAt; 
            set => _createdAt = value; 
        }

        public BaseEntity()
        {
            _createdAt = DateTime.Now;
        }

        /// <summary>
        /// Polymorphism: Virtual method to be overridden by child classes.
        /// </summary>
        public virtual string GetSummary()
        {
            return $"Entity ID: {ID}, Created: {CreatedAt}";
        }
    }
}
