using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace EnigmaMachine_Sarte
{
    public partial class MainWindow : Window
    {
        
        string _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV";
        string _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX"; 
        string _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA"; 
        string _reflector = "YRUHQSLDPXNGOKMIEBFZCWVJAT";

       
        int[] _keyOffset = { 0, 0, 0 }; 
        int[] _initOffset = { 0, 0, 0 };

       
        bool _rotor = false;

        Dictionary<char, char> _plugboard = new Dictionary<char, char>();
        private bool _plugboardSet = false; 

        public MainWindow()
        {
            InitializeComponent();
            InitializePlugboardComboBoxes();
            SetDefaults();
            _rotor = false;
            chkRotorOn.IsChecked = false;
            chkRotorOn.IsEnabled = true;
        }

        private void InitializePlugboardComboBoxes()
        {
            cmbPlugboardFrom.Items.Clear();
            cmbPlugboardTo.Items.Clear();

            foreach (char c in _control)
            {
                cmbPlugboardFrom.Items.Add(c.ToString());
                cmbPlugboardTo.Items.Add(c.ToString());
            }

            cmbPlugboardFrom.SelectedIndex = 0;
            cmbPlugboardTo.SelectedIndex = 0;
        }

        private void InitializePlugboardDisplay()
        {
            plugboardConnections.Items.Clear();

            if (_plugboard.Count == 0)
            {
                plugboardConnections.Items.Add(new PlugboardPair { From = '-', To = '-' });
            }
            else
            {
            
                var addedPairs = new HashSet<string>();
                foreach (var pair in _plugboard)
                {
                    if (pair.Key < pair.Value)
                    {
                        plugboardConnections.Items.Add(new PlugboardPair
                        {
                            From = pair.Key,
                            To = pair.Value
                        });
                    }
                }
            }
        }

        private void DisplayRing(Label displayLabel, string ring)
        {
            string temp = "";
            foreach (char r in ring)
                temp += r + "\t";
            displayLabel.Content = temp;
        }



        private int IndexSearch(string ring, char letter)
        {
            int index = 0;
            for (int x = 0; x < ring.Length; x++)
            {
                if (ring[x] == letter)
                {
                    index = x;
                    break;
                }
            }
            return index;
        }

        private void Window_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                txtInput.Text += " ";
                txtEncrypted.Text += " ";
                txtMirrored.Text += " ";
                return;
            }

            if (e.Key == Key.Back)
            {
                if (txtInput.Text.Length > 0)
                {
                    Rotate(false);
                    txtInput.Text = txtInput.Text.Substring(0, txtInput.Text.Length - 1);
                    txtEncrypted.Text = txtEncrypted.Text.Substring(0, txtEncrypted.Text.Length - 1);
                    txtMirrored.Text = txtMirrored.Text.Substring(0, txtMirrored.Text.Length - 1);
                }
                return;
            }

            if (e.Key.ToString().Length == 1 && char.IsLetter(e.Key.ToString()[0]))
            {
                char inputChar = char.ToUpper(e.Key.ToString()[0]);
                txtInput.Text += inputChar;

                char encryptedChar = Encrypt(inputChar);
                txtEncrypted.Text += encryptedChar;

                char mirroredChar = Mirror(inputChar);
                txtMirrored.Text += mirroredChar;

           
                HighlightKey(inputChar);
                HighlightLight(encryptedChar);

                if (_rotor)
                {
                    Rotate(true);
                    UpdateRotorPositions();
                }
            }
        }

        private async void HighlightKey(char key)
        {
           
            ResetKeyColors();

            foreach (var child in keyboardPanel.Children)
            {
                if (child is StackPanel row)
                {
                    foreach (var control in row.Children)
                    {
                        if (control is Button btn && btn.Content.ToString() == key.ToString())
                        {
                            btn.Background = Brushes.Gold;
                            break;
                        }
                    }
                }
            }

            await System.Threading.Tasks.Task.Delay(300);
            ResetKeyColors();
        }
        private async void HighlightLight(char light)
        {
            
            ResetLightColors();

            foreach (var child in lightboardPanel.Children)
            {
                if (child is StackPanel row)
                {
                    foreach (var control in row.Children)
                    {
                        if (control is Label lbl && lbl.Content.ToString() == light.ToString())
                        {
                            lbl.Background = Brushes.Gold;
                            break;
                        }
                    }
                }
            }

            await System.Threading.Tasks.Task.Delay(300);
            ResetLightColors();
        }


        private void KeyButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null)
            {
                char inputChar = button.Content.ToString()[0];
                txtInput.Text += inputChar;

                char encryptedChar = Encrypt(inputChar);
                txtEncrypted.Text += encryptedChar;

                char mirroredChar = Mirror(inputChar);
                txtMirrored.Text += mirroredChar;

                HighlightKey(inputChar);
                HighlightLight(encryptedChar);

                if (_rotor)
                {
                    Rotate(true);
                    UpdateRotorPositions();
                }
            }
        }

        private void UpdateRotorPositions()
        {
            txtRotor1Pos.Text = _keyOffset[0].ToString();
            txtRotor2Pos.Text = _keyOffset[1].ToString();
            txtRotor3Pos.Text = _keyOffset[2].ToString();

       
            lblRotor1Current.Content = "Current: " + _ring1[_keyOffset[0]];
            lblRotor2Current.Content = "Current: " + _ring2[_keyOffset[1]];
            lblRotor3Current.Content = "Current: " + _ring3[_keyOffset[2]];
        }

        private char Encrypt(char letter)
        {
            char newChar = letter;

            if (_plugboard.Count > 0)
            {
                if (_plugboard.ContainsKey(newChar))
                    newChar = _plugboard[newChar];
                else if (_plugboard.ContainsValue(newChar))
                    newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;
            }

            newChar = _ring1[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring3[IndexSearch(_control, newChar)];

            newChar = _reflector[IndexSearch(_control, newChar)];

            newChar = _ring3[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring1[IndexSearch(_control, newChar)];

            if (_plugboard.Count > 0)
            {
                if (_plugboard.ContainsKey(newChar))
                    newChar = _plugboard[newChar];
                else if (_plugboard.ContainsValue(newChar))
                    newChar = _plugboard.FirstOrDefault(x => x.Value == newChar).Key;
            }

            return newChar;
        }

        private char Mirror(char letter)
        {
            char newChar = Encrypt(letter);

            newChar = _ring3[IndexSearch(_control, newChar)];
            newChar = _ring2[IndexSearch(_control, newChar)];
            newChar = _ring1[IndexSearch(_control, newChar)];

            return newChar;
        }

        private void SetDefaults()
        {
            _control = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            _ring1 = "DMTWSILRUYQNKFEJCAZBPGXOHV";
            _ring2 = "HQZGPJTMOBLNCIFDYAWVEUSRKX";
            _ring3 = "UQNTLSZFMREHDPXKIBVYGJCWOA";
            _keyOffset = new int[] { 0, 0, 0 };

            txtInput.Text = "";
            txtEncrypted.Text = "";
            txtMirrored.Text = "";


            UpdateRotorPositions();
           ResetKeyColors();
            ResetLightColors();
        }

        private void ResetKeyColors()
        {
            var darkControlBackground = (SolidColorBrush)FindResource("DarkControlBackground");
            var darkBorder = (SolidColorBrush)FindResource("DarkBorder");

            foreach (var child in keyboardPanel.Children)
            {
                if (child is StackPanel row)
                {
                    foreach (var control in row.Children)
                    {
                        if (control is Button btn)
                        {
                            btn.Background = darkControlBackground;
                            btn.Foreground = Brushes.Black;
                            btn.BorderBrush = darkBorder;
                        }
                    }
                }
            }
        }

        private void ResetLightColors()
        {
            foreach (var child in lightboardPanel.Children)
            {
                if (child is StackPanel row)
                {
                    foreach (var control in row.Children)
                    {
                        if (control is Label lbl)
                        {
                            lbl.Background = Brushes.Transparent;
                        }
                    }
                }
            }
        }

        // ORIGINAl WHERE THIRD ROTOR MOVES
        private void RotateOriginal(bool forward)
        {
            if (!_rotor) return; // Only rotate if rotor is enabled

            if (forward)
            {
                // Rotate the fast rotor (Rotor 3) first
                _keyOffset[0] = (_keyOffset[0] + 1) % _control.Length;
                _ring3 = MoveValues(true, _ring3);

                // Check if we need to rotate the next rotor (notch position)
                if (_keyOffset[0] == 0) // Assuming notch is at position 0 (like real Enigma)
                {
                    _keyOffset[1] = (_keyOffset[1] + 1) % _control.Length;
                    _ring2 = MoveValues(true, _ring2);

                    // Check if we need to rotate the slow rotor
                    if (_keyOffset[1] == 0) // Assuming notch is at position 0
                    {
                        _keyOffset[0] = (_keyOffset[0] + 1) % _control.Length;
                        _ring1 = MoveValues(true, _ring1);
                    }
                }
            }
            else // Rotating backward (for backspace)
            {
                // First check if we're at position 0 for each rotor
                if (_keyOffset[0] == 0)
                {
                    _keyOffset[0] = _control.Length - 1;
                    _ring1 = MoveValues(false, _ring1);

                    if (_keyOffset[1] == 0)
                    {
                        _keyOffset[1] = _control.Length - 1;
                        _ring2 = MoveValues(false, _ring2);

                        if (_keyOffset[0] == 0)
                        {
                            _keyOffset[0] = _control.Length - 1;
                            _ring1 = MoveValues(false, _ring1);
                            _ring1 = MoveValues(false, _ring1);
                        }
                        else
                        {
                            _keyOffset[0]--;
                            _ring1 = MoveValues(false, _ring1);
                        }
                    }
                    else
                    {
                        _keyOffset[1]--;
                        _ring2 = MoveValues(false, _ring2);
                    }
                }
                else
                {
                    _keyOffset[0]--;
                    _ring3 = MoveValues(false, _ring3);
                }
            }

            UpdateRotorPositions();
        }

        //REVISED WHERE FIRST MOTOR MOVES
        private void Rotate(bool forward)
        {
            if (!_rotor) return; 

            if (forward)
            {
               
                _keyOffset[0] = (_keyOffset[0] + 1) % _control.Length;
                _ring1 = MoveValues(true, _ring1);

              
                if (_keyOffset[0] == 25) 
                {
                   
                    _keyOffset[1] = (_keyOffset[1] + 1) % _control.Length;
                    _ring2 = MoveValues(true, _ring2);

                    if (_keyOffset[1] == 25) 
                    {
                      
                        _keyOffset[2] = (_keyOffset[2] + 1) % _control.Length;
                        _ring3 = MoveValues(true, _ring3);
                    }
                }
          
                else if (_keyOffset[1] == 25)
                {
                 
                    _keyOffset[1] = (_keyOffset[1] + 1) % _control.Length;
                    _ring2 = MoveValues(true, _ring2);

                    _keyOffset[2] = (_keyOffset[2] + 1) % _control.Length;
                    _ring3 = MoveValues(true, _ring3);
                }
            }
            else
            {
               
                if (_keyOffset[0] == 0)
                {
                    _keyOffset[0] = _control.Length - 1; 
                    _ring1 = MoveValues(false, _ring1);

                    
                    if (_keyOffset[1] == 0)
                    {
                        _keyOffset[1] = _control.Length - 1;
                        _ring2 = MoveValues(false, _ring2);

                        if (_keyOffset[2] == 0)
                        {
                            _keyOffset[2] = _control.Length - 1;
                            _ring3 = MoveValues(false, _ring3);
                        }
                        else
                        {
                            _keyOffset[2]--;
                            _ring3 = MoveValues(false, _ring3);
                        }
                    }
                    else
                    {
                        _keyOffset[1]--;
                        _ring2 = MoveValues(false, _ring2);
                    }
                }
                else
                {
                    _keyOffset[0]--;
                    _ring1 = MoveValues(false, _ring1);
                }
            }

            UpdateRotorPositions();
        }

        private string MoveValues(bool forward, string ring)
        {
            if (forward)
            {
            
                return ring.Substring(1) + ring[0];
            }
            else
            {
             
                return ring[ring.Length - 1] + ring.Substring(0, ring.Length - 1);
            }
        }

        private void Rotor1Up_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtRotor1Pos.Text, out int pos) && pos < 25)
            {
                txtRotor1Pos.Text = (pos + 1).ToString();
                RotorPosition_LostFocus(txtRotor1Pos, null);
            }
        }

        private void Rotor1Down_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtRotor1Pos.Text, out int pos) && pos > 0)
            {
                txtRotor1Pos.Text = (pos - 1).ToString();
                RotorPosition_LostFocus(txtRotor1Pos, null);
            }
        }

        private void Rotor2Up_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtRotor2Pos.Text, out int pos) && pos < 25)
            {
                txtRotor2Pos.Text = (pos + 1).ToString();
                RotorPosition_LostFocus(txtRotor2Pos, null);
            }
        }

        private void Rotor2Down_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtRotor2Pos.Text, out int pos) && pos > 0)
            {
                txtRotor2Pos.Text = (pos - 1).ToString();
                RotorPosition_LostFocus(txtRotor2Pos, null);
            }
        }

        private void Rotor3Up_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtRotor3Pos.Text, out int pos) && pos < 25)
            {
                txtRotor3Pos.Text = (pos + 1).ToString();
                RotorPosition_LostFocus(txtRotor3Pos, null);
            }
        }

        private void Rotor3Down_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtRotor3Pos.Text, out int pos) && pos > 0)
            {
                txtRotor3Pos.Text = (pos - 1).ToString();
                RotorPosition_LostFocus(txtRotor3Pos, null);
            }
        }

        private void Rotor_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            TextBox rotorPos = null;
            if (((Button)sender).Tag.ToString() == "1") rotorPos = txtRotor1Pos;
            else if (((Button)sender).Tag.ToString() == "2") rotorPos = txtRotor2Pos;
            else if (((Button)sender).Tag.ToString() == "3") rotorPos = txtRotor3Pos;

            if (rotorPos != null)
            {
                if (e.Delta > 0 && int.Parse(rotorPos.Text) < 25)
                    rotorPos.Text = (int.Parse(rotorPos.Text) + 1).ToString();
                else if (e.Delta < 0 && int.Parse(rotorPos.Text) > 0)
                    rotorPos.Text = (int.Parse(rotorPos.Text) - 1).ToString();

                RotorPosition_LostFocus(rotorPos, null);
            }
        }

        private void RotorPosition_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsDigit(e.Text, 0))
            {
                e.Handled = true;
                return;
            }

            TextBox textBox = sender as TextBox;
            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            if (newText.Length > 0)
            {
                int value;
                if (!int.TryParse(newText, out value) || value < 0 || value > 25)
                {
                    e.Handled = true;
                }
            }
        }

        private void RotorPosition_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                RotorPosition_LostFocus(sender, null);
                e.Handled = true;
            }
        }

        private void RotorPosition_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox != null)
            {
                int value;
                if (int.TryParse(textBox.Text, out value))
                {
                    if (value < 0) textBox.Text = "0";
                    if (value > 25) textBox.Text = "25";

                    if (textBox == txtRotor1Pos)
                    {
                        _initOffset[0] = int.Parse(textBox.Text);
                        _ring1 = InitializeRotors(_initOffset[0], _ring1);
                        _keyOffset[0] = _initOffset[0];
                    }
                    else if (textBox == txtRotor2Pos)
                    {
                        _initOffset[1] = int.Parse(textBox.Text);
                        _ring2 = InitializeRotors(_initOffset[1], _ring2);
                        _keyOffset[1] = _initOffset[1];
                    }
                    else if (textBox == txtRotor3Pos)
                    {
                        _initOffset[2] = int.Parse(textBox.Text);
                        _ring3 = InitializeRotors(_initOffset[2], _ring3);
                        _keyOffset[2] = _initOffset[2];
                    }

                    UpdateRotorPositions();
                }
                else
                {
                    textBox.Text = "0";
                }
            }
        }

        private string InitializeRotors(int initial, string ring)
        {
            string newRing = ring;
            for (int x = 0; x < initial; x++)
                newRing = MoveValues(true, newRing);
            return newRing;
        }

        private void AddPlugboardPair_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPlugboardFrom.SelectedItem == null || cmbPlugboardTo.SelectedItem == null)
                return;

            char fromChar = cmbPlugboardFrom.SelectedItem.ToString()[0];
            char toChar = cmbPlugboardTo.SelectedItem.ToString()[0];

            if (fromChar == toChar)
            {
                MessageBox.Show("Cannot pair a letter with itself.");
                return;
            }

            if (_plugboard.ContainsKey(fromChar) || _plugboard.ContainsKey(toChar) ||
                _plugboard.ContainsValue(fromChar) || _plugboard.ContainsValue(toChar))
            {
                MessageBox.Show("One or both letters are already paired.");
                return;
            }

            _plugboard[fromChar] = toChar;
            _plugboard[toChar] = fromChar;
            _plugboardSet = true;
            chkRotorOn.IsEnabled = true;

           
            plugboardConnections.Items.Add(new PlugboardPair { From = fromChar, To = toChar });

        
            cmbPlugboardFrom.Items.Remove(fromChar.ToString());
            cmbPlugboardFrom.Items.Remove(toChar.ToString());
            cmbPlugboardTo.Items.Remove(fromChar.ToString());
            cmbPlugboardTo.Items.Remove(toChar.ToString());

            if (cmbPlugboardFrom.Items.Count > 0)
                cmbPlugboardFrom.SelectedIndex = 0;
            if (cmbPlugboardTo.Items.Count > 0)
                cmbPlugboardTo.SelectedIndex = 0;
        }

        private void RemovePlugboardPair_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            if (button != null && button.Tag is PlugboardPair pair)
            {
                _plugboard.Remove(pair.From);
                _plugboard.Remove(pair.To);

              
                cmbPlugboardFrom.Items.Add(pair.From.ToString());
                cmbPlugboardFrom.Items.Add(pair.To.ToString());
                cmbPlugboardTo.Items.Add(pair.From.ToString());
                cmbPlugboardTo.Items.Add(pair.To.ToString());

                cmbPlugboardFrom.Items.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));
                cmbPlugboardTo.Items.SortDescriptions.Add(
                    new System.ComponentModel.SortDescription("", System.ComponentModel.ListSortDirection.Ascending));

                plugboardConnections.Items.Remove(pair);

                if (cmbPlugboardFrom.Items.Count > 0)
                    cmbPlugboardFrom.SelectedIndex = 0;
                if (cmbPlugboardTo.Items.Count > 0)
                    cmbPlugboardTo.SelectedIndex = 0;

                if (_plugboard.Count == 0)
                {
                    _plugboardSet = false;
                    chkRotorOn.IsEnabled = false;
                }
            }
        }

     

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            SetDefaults();
            _plugboard.Clear();
            _plugboardSet = false;
            chkRotorOn.IsChecked = false;
            chkRotorOn.IsEnabled = true;
            _rotor = false;

     
            plugboardConnections.Items.Clear();
            InitializePlugboardComboBoxes();

            if (plugboardConnections.Items.Count == 0)
            {
                plugboardConnections.Items.Add(new PlugboardPair { From = '-', To = '-' });
            }
        }

        private void chkRotorOn_Checked_1(object sender, RoutedEventArgs e)
        {
            _rotor = chkRotorOn.IsChecked == true;
            chkRotorOn.IsEnabled = false;
        }
    }
}