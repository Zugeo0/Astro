using AstroLang.Analysis.Parsing;

namespace AstroLang.Runtime
{
    public class Restricted<T>
    {
        public AccessModifier Accessability;
        private T _value;

        public Restricted(AccessModifier accessability, T value)
        {
            Accessability = accessability;
            _value = value;
        }

        public T Unlock(Object? owner)
        {
            return _value;
        }
    }
}
