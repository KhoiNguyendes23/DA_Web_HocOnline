using System.Collections.Generic;
using Do_An_Web_Hoc.Models;

public class ReviewService : IReview
{
    private readonly List<Review> _reviews;

    public ReviewService()
    {
        // Giả lập dữ liệu mẫu, bạn sẽ thay thế bằng cơ sở dữ liệu thực tế sau này
        _reviews = new List<Review>
        {
            new Review { Title = "Tuyệt vời", Content = "Sản phẩm này rất tuyệt vời.", Rating = 5, Author = "Nguyễn Văn A", Date = "2025-03-12" },
            new Review { Title = "Tốt", Content = "Sản phẩm khá tốt, nhưng cần cải thiện thêm.", Rating = 4, Author = "Trần Thị B", Date = "2025-03-11" }
        };
    }

    public IEnumerable<Review> GetReviews()
    {
        return _reviews;
    }

    public void AddReview(Review review)
    {
        _reviews.Add(review); // Ở đây bạn có thể thay bằng lưu vào cơ sở dữ liệu thực tế
    }
}
