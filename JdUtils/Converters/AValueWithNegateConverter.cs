namespace JdUtils.Converters
{
    public abstract class AValueWithNegateConverter : AValueConverter
    {
        protected AValueWithNegateConverter()
            : this(false)
        {
        }

        protected AValueWithNegateConverter(bool negate)
        {
            Negate = negate;
        }

        public bool Negate { get; set; }
    }
}
