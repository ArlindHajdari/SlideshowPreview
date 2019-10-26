using PhotoSlideshow.Models;
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace PhotoSlideshow
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] test_files = Directory.GetFiles(@$"{AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"))}Samples", "*.txt");
            List<Instance> test_instances = new List<Instance>();
            Array.ForEach(test_files, x => { test_instances.Add(Extensions.IO.ReadInput(x)); });


            foreach (Instance instance in test_instances.Take(1))
            {
                Console.WriteLine($"Number of photos: {instance.NumberOfPhotos}\n");

                foreach (Photo photo in instance.Photos)
                {
                    Console.WriteLine($"Orientation: {photo.Orientation}\tTags: {string.Join(',',photo.Tags.ToArray())}");
                }
            }
            Console.ReadKey();
        }
    }
}
