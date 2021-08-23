namespace Crosspoint.Api.Models
{

    /// <summary>
    /// Defines an entry in the mapping diagram layout that is output centric
    /// </summary>
    public class CrosspointMapping
    {

        /// <summary>
        /// Gets or sets the output port number
        /// </summary>
        public int Output { get; set; }

        /// <summary>
        /// Gets or sets the input port that is running video to this output
        /// </summary>
        public int VideoInput { get; set; }

        /// <summary>
        /// Gets or sets the input port that is running audio to this output
        /// </summary>
        public int AudioInput { get; set; }

    }

}