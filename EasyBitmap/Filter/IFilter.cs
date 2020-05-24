namespace Easy.Filter
{
    public interface IFilter
    {
        StatusReporter StatusReporter { get; set; }

        byte[] Filter(ulong width, ulong height, byte[] data);

        byte[] Defilter(ulong width, ulong height, byte[] data);

    }
}
