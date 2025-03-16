namespace Do_An_Web_Hoc.Models
{
    public interface IReview
    {
        IEnumerable<Review> GetReviews();
        void AddReview(Review review);
    }
}
