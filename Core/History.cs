using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet
{
    #region History Model

    public interface IHistoryController
    {
        void Register(string message);
        void Reset();
        void Undo();
        void Redo();
    }

    public class ChangeMessage
    {
        public string Message { get; set; }
        public string Model { get; set; }
    }

    #endregion

    #region History Controller

    public class HistoryController : IHistoryController
    {
        #region Properties

        public IBlockController BlockController { get; private set; }

        #endregion

        #region Fields

        private Stack<ChangeMessage> undos = new Stack<ChangeMessage>();
        private Stack<ChangeMessage> redos = new Stack<ChangeMessage>();

        #endregion

        #region Constructor

        public HistoryController(IBlockController blockController)
        {
            BlockController = blockController;
        }

        #endregion

        #region Undo/Redo Changes

        private async Task<ChangeMessage> CreateChangeMessage(string message)
        {
            var block = BlockController.SerializePage();
            var text = await Task.Run(() => ItemSerializer.SerializeContents(block));
            var change = new ChangeMessage()
            {
                Message = message,
                Model = text
            };
            return change;
        }

        public async void Register(string message)
        {
            var change = await CreateChangeMessage(message);
            undos.Push(change);
            redos.Clear();
        }

        public void Reset()
        {
            undos.Clear();
            redos.Clear();
        }

        public async void Undo()
        {
            if (undos.Count > 0)
            {
                var change = await CreateChangeMessage("Redo");
                redos.Push(change);
                var undo = undos.Pop();
                var block = await Task.Run(() => ItemSerializer.DeserializeContents(undo.Model));
                BlockController.ResetPage();
                BlockController.DeserializePage(block);
            }
        }

        public async void Redo()
        {
            if (redos.Count > 0)
            {
                var change = await CreateChangeMessage("Undo");
                undos.Push(change);
                var redo = redos.Pop();
                var block = await Task.Run(() => ItemSerializer.DeserializeContents(redo.Model));
                BlockController.ResetPage();
                BlockController.DeserializePage(block);
            }
        }

        #endregion
    }

    #endregion
}
