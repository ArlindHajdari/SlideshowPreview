using System;
using System.Collections.Generic;
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

        public void GenerateRandomSolution(List<Photo> photos)
        {
            List<int> photosToSkip = new List<int>();
            for (int i = 0; i < photos.Count; i++)
            {
                if (photosToSkip.Any(x => x == photos[i].Id))
                {
                    continue;
                }

                List<Photo> photosToAdd = new List<Photo>()
                {
                    photos[i]
                };

                if (photos[i].Orientation == Orientation.V)
                {
                    Photo photo = photos.Skip(i + 1).Where(x => x.Orientation.Equals(Orientation.V) && !photosToSkip.Contains(x.Id))
                    .OrderByDescending(x => x.Tags.Where(t => !photos[i].Tags.Contains(t)).Count()).FirstOrDefault();

                    if (photo != null)
                    {
                        photosToAdd.Add(photo);
                        photosToSkip.Add(photo.Id);
                    }
                }
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

        #endregion
    }
}
