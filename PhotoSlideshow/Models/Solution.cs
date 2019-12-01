using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PhotoSlideshow.Models
{
    public class Solution
    {
        public List<Slide> Slides { get; set; }
        public int InterestFactor { get; set; } = int.MinValue;

        public Solution()
        {
            this.Slides = new List<Slide>();
        }

        public Solution(List<Slide> Slides)
        {
            this.Slides = Slides;
        }

        #region [Functions]

        public void HillClimbing(int numberOfIterations)
        {
            Random random = new Random();
            List<int> randomNumbers = new List<int>();
            for (int i = 0; i < this.Slides.Count(); i++)
            {
                randomNumbers.Add(i);
            }

            for (int i = 0; i < numberOfIterations; i++)
            {
                List<Slide> tempSolution = this.Slides;
                List<int> slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();

                Slide tempSlide = tempSolution[slidesToSwap.FirstOrDefault()];
                tempSolution[slidesToSwap.FirstOrDefault()] = tempSolution[slidesToSwap.LastOrDefault()];
                tempSolution[slidesToSwap.LastOrDefault()] = tempSlide;

                int currentInterestFactor = CalculateInterestFactor(tempSolution);
                if (currentInterestFactor > this.InterestFactor)
                {
                    this.Slides = tempSolution;
                    this.InterestFactor = currentInterestFactor;
                }
            }
        }

        public void SimulatedAnnealing(double temperature = 400.0, double alpha = 0.999, double epsilon = 0.001)
        {
            Random random = new Random();
            double maxTemperature = temperature;
            int slideNumber = this.Slides.Count();
            List<int> randomNumbers = new List<int>();

            for (int i = 0; i < slideNumber; i++)
            {
                randomNumbers.Add(i);
            }

            Console.WriteLine($"Initial Values\nalpha: { alpha }, temperature: { temperature }, epsilon: {epsilon}.");

            while (temperature > epsilon)
            {
                temperature *= alpha;
                List<Slide> tempSolution = this.Slides;
                int normalizedValue = (int)Math.Ceiling((temperature / maxTemperature) * slideNumber);

                Console.WriteLine($"Nomalized Value { normalizedValue }\t\tCurrent temperature: { temperature }");

                for (int i = 0; i < normalizedValue; i++)
                {
                    List<int> slidesToSwap = randomNumbers.OrderBy(x => random.Next()).Take(2).ToList();
                    Slide tempSlide = tempSolution[slidesToSwap.FirstOrDefault()];
                    tempSolution[slidesToSwap.FirstOrDefault()] = tempSolution[slidesToSwap.LastOrDefault()];
                    tempSolution[slidesToSwap.LastOrDefault()] = tempSlide;
                }

                int currentInterestFactor = CalculateInterestFactor(tempSolution);
                if (currentInterestFactor > this.InterestFactor)
                {
                    this.Slides = tempSolution;
                    this.InterestFactor = currentInterestFactor;
                }
            }
        }

        public void GenerateRandomSolution(List<Photo> photos)
        {
            Random random = new Random();
            List<int> photosToSkip = new List<int>();

            while (photosToSkip.Count() < photos.Count())
            {
                Photo photo;
                if (photosToSkip.Count() == 0)
                {
                    int randomStart = random.Next(0, photos.Count() - 1);
                    photo = photos.Where(x => randomStart == x.Id).FirstOrDefault();
                }
                else
                {
                    photo = photos.Where(x => !photosToSkip.Contains(x.Id))
                       .OrderByDescending(x =>
                           x.Tags.Where(t => !this.Slides.LastOrDefault().Tags.Contains(t)).Count() +
                           x.Tags.Where(t => this.Slides.LastOrDefault().Tags.Contains(t)).Count())
                       .FirstOrDefault();
                }

                List<Photo> photosToAdd = new List<Photo>()
                {
                    photo
                };

                if (photo.Orientation == Orientation.V)
                {
                    Photo secondPhoto = photos
                        .Where(x => x.Id != photo.Id && x.Orientation.Equals(Orientation.V) && !photosToSkip.Contains(x.Id))
                        .OrderByDescending(x =>
                            x.Tags.Where(t => !photo.Tags.Contains(t)).Count() +
                            x.Tags.Where(t => photo.Tags.Contains(t)).Count())
                        .FirstOrDefault();

                    if (secondPhoto != null)
                    {
                        photosToAdd.Add(secondPhoto);
                        photosToSkip.Add(secondPhoto.Id);
                    }
                }

                photosToSkip.Add(photo.Id);
                this.Slides.Add(new Slide(photosToAdd));
            }
        }

        public int CalculateInterestFactor(List<Slide> slides)
        {
            int interestFactor = 0;
            for (int i = 0; i < slides.Count - 1; i++)
            {
                int commonTags = CalculateCommonSlideTags(slides[i], slides[i + 1]);
                int slideAnotB = CalculateDifferenteSlideTags(slides[i], slides[i + 1]);
                int slideBnotA = CalculateDifferenteSlideTags(slides[i + 1], slides[i]);
                interestFactor += Math.Min(commonTags, Math.Min(slideAnotB, slideBnotA));
            }
            return interestFactor;
        }

        public int CalculateCommonSlideTags(Slide slideA, Slide slideB)
        {
            return slideA.Tags.Where(x => slideB.Tags.Contains(x)).Count();
        }

        public int CalculateDifferenteSlideTags(Slide slideA, Slide slideB)
        {
            return slideA.Tags.Where(x => !slideB.Tags.Contains(x)).Count();
        }

        public void GenerateOutputFile(string filename)
        {
            using (StreamWriter file = new StreamWriter(new FileStream(filename, FileMode.CreateNew)))
            {
                file.WriteLine(this.Slides.Count);
                foreach (Slide slide in this.Slides)
                {
                    file.WriteLine($"{string.Join(" ", slide.Photos.Select(x => x.Id).ToList())}");
                }
            }
        }

        #endregion
    }
}
