using System.Transactions;
using VideoGuide.Models;

namespace VideoGuide.Services
{
    public class Change_Display_Order
    {
        private readonly VideoGuideContext _context;

        public Change_Display_Order(VideoGuideContext context)
        {
            _context = context;
        }
        public void Group (Group group , int DisplayOderDestination)
        {
            if (group.DisplayOrder < DisplayOderDestination)
            {
                List<Group> groups = _context.Groups.Where(order => order.DisplayOrder <= DisplayOderDestination && order.DisplayOrder > group.DisplayOrder).ToList();
                groups.ForEach(group => group.DisplayOrder--);
                group.DisplayOrder = DisplayOderDestination;
                groups.Add(group);

                    _context.Groups.BulkUpdate(groups);
                    _context.SaveChanges();
                
            }
            else if(group.DisplayOrder > DisplayOderDestination)
            {
                List<Group> groups = _context.Groups.Where(order=>order.DisplayOrder >= DisplayOderDestination && order.DisplayOrder < group.DisplayOrder).ToList();
                groups.ForEach(group => group.DisplayOrder++);
                    group.DisplayOrder = DisplayOderDestination;
                groups.Add(group);

                    _context.Groups.BulkUpdate(groups);
                    _context.SaveChanges();

            }
            return;
        }
        public void Tag(Tag tag, int DisplayOderDestination)
        {
            if (tag.DisplayOrder < DisplayOderDestination)
            {
                List<Tag> tags = _context.Tags.Where(order => order.DisplayOrder <= DisplayOderDestination && order.DisplayOrder > tag.DisplayOrder).ToList();
                tags.ForEach(tag => tag.DisplayOrder--);
                tag.DisplayOrder = DisplayOderDestination;
                tags.Add(tag);

                    _context.Tags.BulkUpdate(tags);
                    _context.SaveChanges();

            }
            else if (tag.DisplayOrder > DisplayOderDestination)
            {
                List<Tag> tags = _context.Tags.Where(order => order.DisplayOrder >= DisplayOderDestination && order.DisplayOrder < tag.DisplayOrder).ToList();
                tags.ForEach(tag => tag.DisplayOrder++);
                tag.DisplayOrder = DisplayOderDestination;
                tags.Add(tag);

                    _context.Tags.BulkUpdate(tags);
                    _context.SaveChanges();

            }
            return;
        }
        public void Video(Video video, int DisplayOderDestination)
        {
            if (video.DisplayOrder < DisplayOderDestination)
            {
                List<Video> videos = _context.Videos.Where(order => order.DisplayOrder <= DisplayOderDestination && order.DisplayOrder > video.DisplayOrder).ToList();
                videos.ForEach(video => video.DisplayOrder--);
                video.DisplayOrder = DisplayOderDestination;
                videos.Add(video);

                    _context.Videos.BulkUpdate(videos);
                    _context.SaveChanges();

            }
            else if (video.DisplayOrder > DisplayOderDestination)
            {
                List<Video> videos = _context.Videos.Where(order => order.DisplayOrder >= DisplayOderDestination && order.DisplayOrder < video.DisplayOrder).ToList();
                videos.ForEach(tag => tag.DisplayOrder++);
                video.DisplayOrder = DisplayOderDestination;
                videos.Add(video);

                    _context.Videos.BulkUpdate(videos);
                    _context.SaveChanges();

            }
            return;
        }
    }
}
