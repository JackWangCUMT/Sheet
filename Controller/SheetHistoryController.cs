using Sheet.Item;
using Sheet.Item.Model;
using Sheet.Controller.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sheet.Controller
{
    public class SheetHistoryController : IHistoryController
    {
        #region IoC

        private readonly ISheetController _sheetController;
        private readonly IItemSerializer _itemSerializer;

        public SheetHistoryController(ISheetController sheetController, IItemSerializer itemSerializer)
        {
            this._sheetController = sheetController;
            this._itemSerializer = itemSerializer;
        }

        #endregion

        #region Fields

        private Stack<ChangeItem> undos = new Stack<ChangeItem>();
        private Stack<ChangeItem> redos = new Stack<ChangeItem>();

        #endregion

        #region Factory

        private async Task<ChangeItem> CreateChange(string message)
        {
            var block = _sheetController.SerializePage();
            var text = await Task.Run(() => _itemSerializer.SerializeContents(block));
            var change = new ChangeItem()
            {
                Message = message,
                Model = text
            };
            return change;
        }

        #endregion

        #region IHistoryController

        public async void Register(string message)
        {
            var change = await CreateChange(message);
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
                try
                {
                    var change = await CreateChange("Redo");
                    redos.Push(change);
                    var undo = undos.Pop();
                    var block = await Task.Run(() => _itemSerializer.DeserializeContents(undo.Model));
                    _sheetController.ResetPage();
                    _sheetController.DeserializePage(block);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        public async void Redo()
        {
            if (redos.Count > 0)
            {
                try
                {
                    var change = await CreateChange("Undo");
                    undos.Push(change);
                    var redo = redos.Pop();
                    var block = await Task.Run(() => _itemSerializer.DeserializeContents(redo.Model));
                    _sheetController.ResetPage();
                    _sheetController.DeserializePage(block);
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message);
                    Debug.Print(ex.StackTrace);
                }
            }
        }

        #endregion
    }
}
