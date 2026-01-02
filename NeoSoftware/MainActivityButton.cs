using System;
using Android.Graphics;
using Android.Widget;
using Java.Interop;
using MathNet.Numerics.LinearAlgebra;
using Neo.Services;
using Neo.Utilities;
using NeoSoftware.Services;
using NeoSoftware.Utilities;
using Button = Android.Widget.Button;
using Switch = Android.Widget.Switch;
using TextAlignment = Android.Views.TextAlignment;
using View = Android.Views.View;


namespace NeoSoftware
{
    public partial class MainActivity
    {
        private Button _transpose;
        private Button _inverse;
        private Button _det;
        private Button _rank;
        private Button _exponentiation;
        private Button _solveMatrix;
        private GridLayout _gridLayoutMatrix;
        private EditText _rowsCount;
        private EditText _columnsCount;
        private TextView _resultOutput;
        private TextView _resultInput;
        private TextView _resultTitle;
        private Button _submitSize;
        private Matrix<double> _matrix;
        private Matrix<double> _inputMatrix;
        private string _inputEquations;
        private string _equations;
        private Switch _isEquationsSwitch;
        private readonly GridSize _gridSize = new GridSize();

        /// <summary>
        /// brings together all configurations for change "view of input fields" to "equation view"
        /// </summary>
        private void ConfigureRenderingInputFields()
        {
            ConfigureIsEquationsSwitch();
            ConfigureRowsColumnsCount();
            ConfigureGrid();
        }

        /// <summary>
        /// initializes instances of all elements
        /// </summary>
        private void InitAllElements()
        {
            GetButtons();
            GetRowsColumnsCountFields();
            GetEquationSwitchFields();
        }

        /// <summary>
        /// initializes instances of fields for changing "rendering view"
        /// </summary>
        private void GetEquationSwitchFields()
        {
            _isEquationsSwitch = FindViewById<Switch>(Resource.Id.is_equations_switch);
        }

        /// <summary>
        /// initializes instances for changing size of matrix
        /// </summary>
        private void GetRowsColumnsCountFields()
        {
            _rowsCount = FindViewById<EditText>(Resource.Id.rows_count);
            _columnsCount = FindViewById<EditText>(Resource.Id.columns_count);
            _submitSize = FindViewById<Button>(Resource.Id.submit_size);
        }

        /// <summary>
        /// initializes instances of buttons
        /// </summary>
        private void GetButtons()
        {
            _transpose = FindViewById<Button>(Resource.Id.transpose_btn);
            _inverse = FindViewById<Button>(Resource.Id.inverse_btn);
            _det = FindViewById<Button>(Resource.Id.det_btn);
            _rank = FindViewById<Button>(Resource.Id.rank_btn);
            _exponentiation = FindViewById<Button>(Resource.Id.exp_btn);
            _solveMatrix = FindViewById<Button>(Resource.Id.solve_manual_btn);

            ConfigureButtons();
        }

        /// <summary>
        /// configures and renders grid in "equation-view"
        /// </summary>
        private void ConfigureEquationsGrid()
        {
            _isEquationsSwitch.Checked = EquationValueStorage.IsEquation;
            _gridLayoutMatrix.RemoveAllViews();
            _gridLayoutMatrix.RowCount = _gridSize.RowCount = int.Parse(_rowsCount.Text);
            _gridLayoutMatrix.ColumnCount = 1;
            RenderGridLayout(childWidth: 900);
        }

        /// <summary>
        /// sets behavior of grid's input fields by changing value CheckedChange of <see cref="_isEquationsSwitch"/>
        /// </summary>
        private void ConfigureIsEquationsSwitch()
        {
            _isEquationsSwitch.CheckedChange += delegate
            {
                EquationValueStorage.IsEquation = _isEquationsSwitch.Checked;
                RenderMainInputElement();
            };
        }

        /// <summary>
        /// renders matrix if <see cref="_isEquations"/> is false and fields for equations if true
        /// </summary>
        /// <param name="calledRCCConf">defines calling this method from <see cref="ConfigureRowsColumnsCount"/></param>
        private void RenderMainInputElement(bool calledRCCConf = false)
        {
            if (EquationValueStorage.IsEquation)
                ConfigureEquationsGrid();
            else
            {
                if (!calledRCCConf)
                    ConfigureRowsColumnsCount();
                ConfigureGrid();
            }
        }

        /// <summary>
        /// configures buttons event OnClick
        /// </summary>
        private void ConfigureButtons()
        {
            _transpose.Click += delegate
            {
                SetMatrix(_gridLayoutMatrix);
                if (Error.Message != null)
                    return;

                ShowResult(_inputMatrix, _matrix.Transpose(), ResultKind.Transpose);
            };
            _inverse.Click += delegate
            {
                SetMatrix(_gridLayoutMatrix);
                if (Error.Message != null)
                    return;

                if (!_matrix.IsSquare())
                {
                    OpenExceptionWindow();
                    return;
                }

                ShowResult(_inputMatrix, _matrix.Inverse(), ResultKind.Reverse);
            };
            _det.Click += delegate
            {
                SetMatrix(_gridLayoutMatrix);
                if (Error.Message != null)
                    return;
                if (!_matrix.IsSquare())
                {
                    OpenExceptionWindow();
                    return;
                }

                ShowResult(_inputMatrix.GetMatrixValue(), _matrix.Determinant().ToString(),
                    ResultKind.Determinant);
            };
            _rank.Click += delegate
            {
                SetMatrix(_gridLayoutMatrix);
                if (Error.Message != null)
                    return;

                ShowResult(_inputMatrix.GetMatrixValue(), _matrix.Rank().ToString(), ResultKind.Rank);
            };
            _exponentiation.Click += delegate
            {
                SetMatrix(_gridLayoutMatrix);
                if (Error.Message != null)
                    return;
                if (!_matrix.IsSquare())
                {
                    OpenExceptionWindow();
                    return;
                }

                ShowResult(_matrix,
                    _inputMatrix.Power(int.Parse(FindViewById<EditText>(Resource.Id.exp_value).Text)),
                    ResultKind.Exponentiation);
            };
            _solveMatrix.Click += delegate
            {
                if (EquationValueStorage.IsEquation)
                {
                    SetEquations(_gridLayoutMatrix);
                    _solver.Solve(_equations);
                    ShowResult(_inputEquations.Replace(Parser.SplitSymbol, '\n'), _solver.ToString(),
                        ResultKind.Solve);
                }
                else
                {
                    SetMatrix(_gridLayoutMatrix);
                    if (Error.Message != null)
                        return;
                    if (!_matrix.IsGaussianMatrix())
                    {
                        OpenExceptionWindow();
                        return;
                    }

                    _solver.Solve(_matrix);
                    ShowResult(_inputMatrix.GetMatrixValue(), _solver.ToString(), ResultKind.Solve);
                }
            };
        }

        /// <summary>
        /// sets default values for size of grid and fields for changing size of grid
        /// </summary>
        private void ConfigureRowsColumnsCount()
        {
            _rowsCount.Text = _gridSize.RowCount.ToString();
            _columnsCount.Text = _gridSize.ColumnCount.ToString();

            if (_rowsCount.Text == string.Empty)
                _rowsCount.Text = "3";
            if (_columnsCount.Text == string.Empty)
                _columnsCount.Text = "3";

            _submitSize.Click += (sender, args) => { RenderMainInputElement(true); };
        }

        /// <summary>
        /// validates rows and columns count of grid
        /// </summary>
        /// <param name="rows">count of grid's rows</param>
        /// <param name="columns">count of grid's columns</param>
        /// <returns></returns>
        private bool ValidRowsColumnsCount(int rows, int columns)
        {
            return rows <= 6 && rows > 1 && columns <= 6 && columns > 1;
        }

        /// <summary>
        /// sets default grid
        /// </summary>
        private void ConfigureGrid()
        {
            if (!ValidRowsColumnsCount(int.Parse(_rowsCount.Text), int.Parse(_columnsCount.Text)))
                return;

            _gridLayoutMatrix = FindViewById<GridLayout>(Resource.Id.matrix_grid);
            _gridLayoutMatrix.RemoveAllViewsInLayout();
            _gridLayoutMatrix.RowCount = _gridSize.RowCount = int.Parse(_rowsCount.Text);
            _gridLayoutMatrix.ColumnCount = _gridSize.ColumnCount = int.Parse(_columnsCount.Text);

            if (_inputMatrix.GetGridLayout(_gridLayoutMatrix, this) is null)
                RenderGridLayout();
        }

        /// <summary>
        /// renders grid layout with specified parameters of its children and rows/columns count
        /// </summary>
        /// <param name="text">default text of grid's children</param>
        /// <param name="childHeight">height of grid's children</param>
        /// <param name="childWidth">width of grid's children</param>
        private void RenderGridLayout(string text = "", int childHeight = 100, int childWidth = 150)
        {
            var equations = _inputEquations?.Split(Parser.SplitSymbol);
            for (var i = 0; i < _gridLayoutMatrix.RowCount; i++)
            {
                for (var j = 0; j < _gridLayoutMatrix.ColumnCount; j++)
                {
                    using var child = CreateEditTextInstance(childHeight,
                        childWidth,
                        EquationValueStorage.IsEquation
                            ? equations?[i]
                            : text);
                    _gridLayoutMatrix.AddView(child, i + j);
                }
            }
        }

        private EditText CreateEditTextInstance(int childHeight, int childWidth, string text = "")
        {
            var child = new EditText(this);
            if (EquationValueStorage.IsEquation)
            {
                child = new EditText(this)
                {
                    Text = text,
                    TextSize = 18,
                    Typeface = Typeface.DefaultBold,
                };
            }
            else
            {
                child = new EditText(this)
                {
                    TextSize = 18,
                    TextAlignment = TextAlignment.Center,
                    Typeface = Typeface.DefaultBold,
                };
            }

            child.SetTextColor(new Color(168, 170, 177));
            child.SetHeight(childHeight);
            child.SetWidth(childWidth);

            return child;
        }

        private void OpenResultWindow(string title, string input, string output)
        {
            SetContentView(Resource.Layout.result);
            _resultTitle = FindViewById<TextView>(Resource.Id.result_title);
            _resultInput = FindViewById<TextView>(Resource.Id.result_input);
            _resultOutput = FindViewById<TextView>(Resource.Id.result_output);

            _resultTitle.Text = title;
            _resultInput.Text += $"\n{input}";
            _resultOutput.Text += $"\n{output}";
        }

        private void OpenExceptionWindow()
        {
            SetContentView(Resource.Layout.exception);
            var message = FindViewById<TextView>(Resource.Id.message);
            var innerMessage = FindViewById<TextView>(Resource.Id.innerMessage);

            message.Text += $"\n{Error.Message}";
            innerMessage.Text += $"\n{Error.InnerMessage}";
        }

        [Export("Solve")]
        public void OpenConfirmationDialog(View view)
        {
            _detectSwitch.Checked = _detect = false;
            _confirmationAlertDialog.Show();
        }

        [Export("Confirm")]
        public void ConfirmDataInput(View view)
        {
            _detectSwitch.Checked = _detect;
            _confirmationAlertDialog.Cancel();
            _output.Text = _solver.Solve(_tessOutput.Text);
        }

        [Export("BackToRecognition")]
        public void BackToRecognitionBtn(View view)
        {
            _isLoadMain = true;
            SetContentView(Resource.Layout.activity_main);
            BuildUi();
        }

        [Export("BackToManual")]
        public void BackToManualBtn(View view)
        {
            _isLoadMain = false;
            SetContentView(Resource.Layout.activity_manual);
            BuildUi();
            RenderMainInputElement();
        }

        private void SetMatrix(GridLayout gridLayout)
        {
            var matrix = gridLayout.GetMatrix();
            if (matrix is null)
            {
                OpenExceptionWindow();
                return;
            }

            _matrix = matrix;

            var inputMatrix = gridLayout.GetMatrix();
            if (inputMatrix is null)
            {
                OpenExceptionWindow();
                return;
            }

            Error.Message = null;
            _inputMatrix = inputMatrix;
        }

        private void SetEquations(GridLayout gridLayout)
        {
            _equations = gridLayout.GetEquations();
            _inputEquations = gridLayout.GetEquations();
        }

        private void ShowResult(Matrix<double> input, Matrix<double> output, ResultKind resultKind)
            => OpenResultWindow(resultKind.ToString(), input.GetMatrixValue(), output.GetMatrixValue());

        private void ShowResult(string input, string output, ResultKind resultKind)
            => OpenResultWindow(resultKind.ToString(), input, output);
    }
}

#region equation view

// "equation view" is a type of input fields rendering
// when view renders instead of like a matrix, 
// renders like an long input fields

#endregion