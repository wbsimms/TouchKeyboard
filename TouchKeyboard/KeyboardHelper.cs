using System.Collections;
using Gadgeteer.Modules.GHIElectronics;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using TouchKeyboard;

namespace GadgeteerHelper
{
    public class KeyboardHelper
    {
        private KeyStateContext _keyState;
        private Display_T35 display;
        private Font font;
        private Text displayText;
        private IList keys = new ArrayList();

        public delegate void TextChangedEventHander(object sender, TextChangedEventArgs args);
        public event TextChangedEventHander TextChanged;
        public delegate void EnterPressedEventHander(object sender, EnterPressedEventArgs args);
        public event EnterPressedEventHander EnterPressed;

        StackPanel spacer = new StackPanel(Orientation.Horizontal);
        StackPanel spacer1 = new StackPanel(Orientation.Horizontal);
        StackPanel keysRow1 = new StackPanel(Orientation.Horizontal);
        StackPanel keysRow2 = new StackPanel(Orientation.Horizontal);
        StackPanel keysRow3 = new StackPanel(Orientation.Horizontal);
        StackPanel keysRow4 = new StackPanel(Orientation.Horizontal);
        StackPanel keysRow5 = new StackPanel(Orientation.Horizontal);
        StackPanel keysRow6 = new StackPanel(Orientation.Horizontal);

        public KeyboardHelper(Display_T35 display, Font font)
        {
            this.display = display;
            this.font = font;
            this.displayText = new Text(font,"");
            spacer.Height = 2;
            spacer1.Height = 2;
            Init();
        }

        public void Init()
        {
            Window window = display.WPFWindow;
            Panel mainPanel = new Panel();
            window.Child = mainPanel;
            StackPanel sp = new StackPanel(Orientation.Vertical);
            mainPanel.Children.Add(sp);

            sp.Children.Add(spacer);
            StackPanel dispalySP = new StackPanel(Orientation.Horizontal);
            sp.Children.Add(dispalySP);
            dispalySP.Children.Add(displayText);

            sp.Children.Add(spacer1);
            sp.Children.Add(keysRow1);
            sp.Children.Add(keysRow2);
            sp.Children.Add(keysRow3);
            sp.Children.Add(keysRow4);
            sp.Children.Add(keysRow5);
            sp.Children.Add(keysRow6);
            keysRow1.HorizontalAlignment = HorizontalAlignment.Center;
            keysRow2.HorizontalAlignment = HorizontalAlignment.Center;
            keysRow3.HorizontalAlignment = HorizontalAlignment.Center;
            keysRow4.HorizontalAlignment = HorizontalAlignment.Center;
            keysRow5.HorizontalAlignment = HorizontalAlignment.Center;
            keysRow6.HorizontalAlignment = HorizontalAlignment.Center;

            UnShiftKeys(false);

            Key shiftKey = new Key(font, "Shift");
            shiftKey.keyPressedHandler += keyPressedHandler;
            Key spaceKey = new Key(font, "Space");
            spaceKey.keyPressedHandler += keyPressedHandler;
            Key delKey = new Key(font, "Delete");
            delKey.keyPressedHandler += keyPressedHandler;
            keysRow5.Children.Add(shiftKey.RenderKey());
            keysRow5.Children.Add(spaceKey.RenderKey());
            keysRow5.Children.Add(delKey.RenderKey());
            Key enterKey = new Key(font, "Enter");
            enterKey.keyPressedHandler += keyPressedHandler;
            keysRow6.Children.Add(enterKey.RenderKey());


            _keyState = new KeyStateContext(this);
        }

        private Key GetKeyAndAddToList(string text)
        {
            Key k = new Key(font,text);
            k.keyPressedHandler += keyPressedHandler;
            keys.Add(k);
            return k;
        }

        void keyPressedHandler(object sender, KeyPressedEventArgs args)
        {
            if (args.KeyPressed == "Shift")
            {
                _keyState.SwitchState();
            }
            else if (args.KeyPressed == "Space")
            {
                displayText.TextContent = displayText.TextContent + " ";
                OnTextChanged(sender);
            }
            else if (args.KeyPressed == "Enter")
            {
                OnTextChanged(sender);
                OnEnterPressed(sender);
            }
            else if (args.KeyPressed == "Delete")
            {
                if (displayText.TextContent.Length == 0) return;
                displayText.TextContent = displayText.TextContent.Substring(0,displayText.TextContent.Length-1);
                OnTextChanged(sender);
            }
            else
            {
                displayText.TextContent = displayText.TextContent + args.KeyPressed;
                OnTextChanged(sender);
            }
        }

        private void OnEnterPressed(object sender)
        {
            if (EnterPressed != null)
            {
                EnterPressed(sender, new EnterPressedEventArgs(displayText.TextContent));
            }
        }

        private void OnTextChanged(object sender)
        {
            if (TextChanged != null)
            {
                TextChanged(sender, new TextChangedEventArgs(displayText.TextContent));
            }
        }

        private void UnShiftKeys(bool needRemoval = true)
        {
            if (needRemoval)
            {
                RemoveKeys();
            }

            AddKeys(keysRow1, "1234567890");
            AddKeys(keysRow2, "qwertyuiop");
            AddKeys(keysRow3, "asdfghjkl");
            AddKeys(keysRow4, "zxcvbnm");
        }

        protected void ShiftKeys()
        {
            RemoveKeys();

            AddKeys(keysRow1, "!@#$%^&*()");
            AddKeys(keysRow2, "QWERTYUIOP");
            AddKeys(keysRow3, "ASDFGHJKL");
            AddKeys(keysRow4, "ZXCVBNM");
        }

        private void AddKeys(StackPanel keysRow, string keys)
        {
            foreach (var character in keys)
            {
                keysRow.Children.Add(GetKeyAndAddToList(character.ToString()).RenderKey());
            }
        }

        private void RemoveKeys()
        {
            foreach (Key k in keys)
            {
                k.keyPressedHandler -= keyPressedHandler;
                k.Dispose();
            }
            keysRow1.Children.Clear();
            keysRow2.Children.Clear();
            keysRow3.Children.Clear();
            keysRow4.Children.Clear();
        }


        private interface IKeyState
        {
            void CreateKeys();
        }

        private class KeyStateShifted : IKeyState
        {
            private KeyboardHelper _parent;
            public KeyStateShifted(KeyboardHelper parent)
            {
                _parent = parent;
            }

            public void CreateKeys()
            {
                _parent.ShiftKeys();
            }
        }

        private class KeyStateUnshifted : IKeyState
        {
            private KeyboardHelper _parent;
            public KeyStateUnshifted(KeyboardHelper parent)
            {
                _parent = parent;
            }

            public void CreateKeys()
            {
                _parent.UnShiftKeys();
            }
        }

        private class KeyStateContext
        {
            private IKeyState[] _keyStates;
            private IKeyState _currentKeyState;

            public KeyStateContext(KeyboardHelper parent)
            {
                _keyStates = new IKeyState[] { new KeyStateUnshifted(parent), new KeyStateShifted(parent) };
                _currentKeyState = _keyStates[0];
            }

            public void SwitchState()
            {
                if (_currentKeyState is KeyStateUnshifted)
                {
                    _currentKeyState = _keyStates[1];
                }
                else
                {
                    _currentKeyState = _keyStates[0];
                }
                _currentKeyState.CreateKeys();
            }
        }
    }
}