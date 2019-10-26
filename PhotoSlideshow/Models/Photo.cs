using System;
using System.Collections.Generic;
using System.Text;

namespace PhotoSlideshow.Models
{
    public class Photo
    {
        public Photo(Orientation orientation, List<string> tags)
        {
            Orientation = Orientation;
            Tags = tags;
        }
        public Orientation Orientation { get; set; }
        public List<string> Tags { get; set; }
    }

    public enum Orientation
    {
        V,
        H
    }
}
