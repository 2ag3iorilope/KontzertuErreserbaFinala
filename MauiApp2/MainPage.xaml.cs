using MySql.Data.MySqlClient;

namespace MauiApp2
{
    public partial class MainPage : ContentPage
    {
        private readonly DatabaseService _databaseService = new DatabaseService();

        public MainPage()
        {
            InitializeComponent();
            //SetWindowSize();
        }

        /// <summary>
        /// Dnia-ren testua ladtu den konprobatzen duen metodoa, dni hau balidatzeko
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEntrydniTextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender == DNIEntry)

            {
                ValidateDNI(DNIEntry.Text);
            }
        }


        void SetWindowSize()
        {
            var mainWindow = Application.Current?.MainPage as Page;
            mainWindow.WidthRequest = 800;
            mainWindow.HeightRequest = 600;
        }

        /// <summary>
        /// Konexioa egiteko botoiaren emtodoa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>



        /// <summary>
        /// Aukeratutako Herriaren  Erreserba kopurua lortzen duen metodoa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnCitySelected(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                var selectedCity = (sender as RadioButton)?.Content.ToString();
                int reservas = await _databaseService.GetReservasCountAsync(selectedCity);
                reservasLabel.Text = $"Erreserbak: {reservas}";
            }
        }
        /// <summary>
        /// Testuko balioak gaizki sartuz gero testua konpontzen duen emtodoa
        /// Erreserbatzeko botoia aktibatzen du eremu guztiak betetzean
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEntryTextChanged(object sender, TextChangedEventArgs e)
        {

            var entry = sender as Entry;
            if (entry == IzenaEntry || entry == AbizenaEntry)
            {
                if (!string.IsNullOrEmpty(entry.Text) && !IsTextAlphabetic(entry.Text))
                {

                    entry.Text = e.OldTextValue;
                }

            }
            if (entry == KantitateaEntry)
            {
                if (!string.IsNullOrEmpty(entry.Text) && !IsTextNumeric(entry.Text))
                {

                    entry.Text = e.OldTextValue;
                }

            }


            bool isFormFilled = !string.IsNullOrWhiteSpace(IzenaEntry.Text) &&
                        !string.IsNullOrWhiteSpace(AbizenaEntry.Text) &&
                        !string.IsNullOrWhiteSpace(DNIEntry.Text) &&
                        !string.IsNullOrWhiteSpace(KantitateaEntry.Text);


            ErreserbaBotoia.IsEnabled = isFormFilled;

        }

        /// <summary>
        /// Konrpobatu testua alfabetikoa den
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsTextAlphabetic(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsLetter(c))
                    return false;
            }
            return true;
        }
        /// <summary>
        /// Konprobatu testua zenbakiak diren
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private bool IsTextNumeric(string text)
        {
            foreach (char c in text)
            {
                if (!char.IsDigit(c))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Aplikaziotik Irten
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIrtenButtonClicked(object sender, EventArgs e)
        {
            Application.Current!.Quit();
        }

        /// <summary>
        /// Hautatukako herriaren ID-a lortzen du
        /// </summary>
        /// <returns> Herriaren id-a</returns>
        private int GetCityId()
        {
            if (MadridRadioButton.IsChecked) return 1;
            if (BarcelonaRadioButton.IsChecked) return 2;
            if (BilbaoRadioButton.IsChecked) return 3;
            return 0;
        }
        /// <summary>
        /// Dni-a balidatzeko metodoa
        /// </summary>
        /// <param name="dni"></param>
        private void ValidateDNI(string dni)
        {
            if (dni.Length < 9)
            {

                DNIEntry.TextColor = Colors.Red;
                statusLabel.Text = "DNI ez da zuzena";
                return;
            }

            string dniNumberPart = dni.Substring(0, 8);
            char dniLetter = dni[8];

            if (int.TryParse(dniNumberPart, out _) && char.IsLetter(dniLetter))
            {

                string letters = "TRWAGMYFPDXBNJZSQVHLCKE";
                int index = int.Parse(dniNumberPart) % 23;
                char correctLetter = letters[index];

                if (char.ToUpper(dniLetter) == correctLetter)
                {

                    DNIEntry.TextColor = Colors.Green;
                    statusLabel.Text = "DNI zuzena";
                }
                else
                {

                    DNIEntry.TextColor = Colors.Red;
                    statusLabel.Text = "DNI ez da zuzena";
                }
            }
            else
            {
                DNIEntry.TextColor = Colors.Red;
                statusLabel.Text = "DNI ez da zuzena";
            }
        }


        /// <summary>
        /// Erreserba gehitzko botoiaren metodoa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnErreserbaButtonClicked(object sender, EventArgs e)
        {
            string connectionString = "Server=localhost;Port=3306;Database=kontzertuerreserba;User Id=root;Password=mysql;";
            int cityId = GetCityId();
            string izena = IzenaEntry.Text;
            string abizena = AbizenaEntry.Text;
            string dni = DNIEntry.Text;
            int kantitatea = int.Parse(KantitateaEntry.Text);
            string getLastIdQuery = "SELECT MAX(idErreserbak) FROM erreserbak";


            try
            {
                int newId = 1;
                using (MySqlConnection connection = new MySqlConnection(connectionString))
                {
                    await connection.OpenAsync();

                    using (MySqlCommand getIdCommand = new MySqlCommand(getLastIdQuery, connection))
                    {
                        var result = await getIdCommand.ExecuteScalarAsync();
                        if (result != DBNull.Value)
                        {
                            newId = Convert.ToInt32(result) + 1;
                        }
                    }

                    string query = "INSERT INTO erreserbak (idErreserbak, DNI, Abizena, Izena, Kantitatea, Kontzertuak_idKontzertua) " +
                                   "VALUES (@idErreserbak, @dni, @abizena, @izena, @kantitatea, @concierto_id)";

                    using (MySqlCommand insertCommand = new MySqlCommand(query, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@idErreserbak", newId);
                        insertCommand.Parameters.AddWithValue("@dni", dni);
                        insertCommand.Parameters.AddWithValue("@abizena", abizena);
                        insertCommand.Parameters.AddWithValue("@izena", izena);
                        insertCommand.Parameters.AddWithValue("@kantitatea", kantitatea);
                        insertCommand.Parameters.AddWithValue("@concierto_id", cityId);

                        await insertCommand.ExecuteNonQueryAsync();
                        reservasLabel.Text = "Erreserba eginda!";
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Errorea", $"Erreserba ezin izan da burutu: {ex.Message}", "Ados");
            }
        }


    }
}