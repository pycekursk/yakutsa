using static yakutsa.Models.Enums;

namespace yakutsa.Models
{
  public class Message : BaseModel
  {
    private string? text;
    public virtual string? Text { get => text; set => text = value; }
    public MessageType MessageType { get; set; }

    public Message(string text, MessageType messageType)
    {
      this.Text = text;
      MessageType = messageType;
    }
  }
}
