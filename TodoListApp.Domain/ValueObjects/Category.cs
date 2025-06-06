namespace TodoListApp.Domain.ValueObjects
{
    public class Category
    {
        public string Name { get; }

        private Category(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name cannot be empty.");
            Name = name;
        }

        public static Category Create(string name)
        {
            return new Category(name);
        }

        public override bool Equals(object obj)
        {
            return obj is Category other && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Name.ToLowerInvariant().GetHashCode();
        }

        public override string ToString() => Name;
    }
}
