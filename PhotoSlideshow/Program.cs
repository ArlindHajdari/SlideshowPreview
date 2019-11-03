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
            int file_to_read = 0;
            string[] files = Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\Samples", "*.txt");

            List<Slide> slides = new List<Slide>();
            Instance instance = Extensions.IO.ReadInput(files[file_to_read]);
            //files.Select(x => { test_instances.Add(Extensions.IO.ReadInput(x)); return x; }).ToList();

            Console.WriteLine($"Number of photos: {instance.NumberOfPhotos}\n");
            foreach (Photo item in instance.Photos)
            {
                List<Photo> photos = new List<Photo>
                {
                    item
                };
                slides.Add(new Slide(photos));
                Console.WriteLine($"Orientation: {item.Orientation}\tTags: {string.Join(", ", item.Tags.ToArray())}");
            }

            Solution solution = new Solution()
            {
                Slides = slides
            };

            Console.WriteLine($"\nInterest Factor: { solution.InterestFactor }");
            Console.ReadKey();
        }
    }
}
