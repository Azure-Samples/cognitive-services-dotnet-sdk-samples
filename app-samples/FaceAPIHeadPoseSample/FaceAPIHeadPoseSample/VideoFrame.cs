using OpenCvSharp;
using System;

namespace FaceAPIHeadPoseSample
{
    public struct VideoFrameMetadata
    {
        public DateTime Timestamp;
        public int Index;
    }

    /// <summary> A video frame produced by the <see cref="FrameGrabber{AnalysisResultType}"/>.
    ///     This class encapsulates the image, any metadata, and also allows the user to attach
    ///     some arbitrary data to each frame as it flows through the pipeline. </summary>
    public class VideoFrame
    {
        /// <summary> Constructor. </summary>
        /// <param name="image">    The image captured by the camera. </param>
        /// <param name="metadata"> The metadata. </param>
        public VideoFrame(Mat image, VideoFrameMetadata metadata)
        {
            Image = image;
            Metadata = metadata;
        }

        /// <summary> Gets the image for the frame. </summary>
        /// <value> The image. </value>
        public Mat Image { get; }

        /// <summary> Gets the frame's metadata. </summary>
        /// <value> The metadata. </value>
        public VideoFrameMetadata Metadata { get; }

        /// <summary> Gets or sets the frame's "user data". </summary>
        /// <value> Any additional data that the user would like to attach to a video frame. </value>
        public object UserData { get; set; } = null;
    }
}
