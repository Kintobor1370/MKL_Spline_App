using Xunit;
using System;
using System.Linq;
using System.ComponentModel;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.Legends;
using Model;
using ViewModel;
using FluentAssertions;

namespace ViewModelTests
{
    public class TestErrorReporter : IErrorReporter
    {
        public bool There_Was_An_Error { get; set; } = false;
        public void ReportError(string mes)
        {
            There_Was_An_Error = true;
        }
    }

    public class PropertyChangedReporter
    {
        public bool Event { get; private set; } = false;
        public void OnPropertyChange(object sender, PropertyChangedEventArgs e)
        {
            Event = true;
        }
        public void Reset()
        {
            Event = false;
        }
    }

    //______________________________________________________MainViewModel CLASS TESTS______________________________________________________
    public class MainViewModelTest
    {
        //.................................Constructor test
        [Fact]
        public void ConstructorTest()
        {
            var err = new TestErrorReporter();
            var main = new MainViewModel(err);
            
            main.NonUniformNum.Should().Be(2);
            main.UniformNum.Should().Be(2);
            main.Min.Should().Be(0);
            main.Max.Should().Be(0);
            main.Der1Left.Should().Be(1);
            main.Der1Right.Should().Be(1);
            main.Der2Left.Should().Be(0);
            main.Der2Right.Should().Be(0);
            main.Function.Should().Be(SPf.Random);
        }

        //.................................Validations and CanExecute test
        [Fact]
        public void Validation_CanExecute_Test()
        {
            var err = new TestErrorReporter();
            var main = new MainViewModel(err);

            main["NonUniformNum"].Should().Be("Число узлов должно быть больше 2");
            main["Max"].Should().Be("Левый конец отрезка должен быть меньше правого");
            main["UniformNum"].Should().Be("Число узлов должно быть больше 2");
            main.MakeMD.CanExecute(main).Should().BeFalse();
            main.MakeSD.CanExecute(main).Should().BeFalse();

            main.NonUniformNum = 20;
            main.Min = 10;
            main.Max = 125;

            main["NonUniformNum"].Should().Be(String.Empty);
            main["Max"].Should().Be(String.Empty);
            main["UniformNum"].Should().Be("Число узлов должно быть больше 2");
            main.MakeMD.CanExecute(main).Should().BeTrue();
            main.MakeSD.CanExecute(main).Should().BeFalse();

            main.UniformNum = 10;

            main["NonUniformNum"].Should().Be(String.Empty);
            main["Max"].Should().Be(String.Empty);
            main["UniformNum"].Should().Be(String.Empty);
            main.MakeMD.CanExecute(main).Should().BeTrue();
            main.MakeSD.CanExecute(main).Should().BeTrue();
        }

        //.................................Data update test
        [Fact]
        public void UpdateDataTest()
        {
            var err = new TestErrorReporter();
            var main = new MainViewModel(err);

            main.NonUniformNum = 20;
            main.Min = 10; main.Max = 125;
            var new_event_reporter = new PropertyChangedReporter();
            main.PropertyChanged += new_event_reporter.OnPropertyChange;
            main.MakeMD.Execute(main);
            new_event_reporter.Event.Should().BeTrue();

            new_event_reporter.Reset();
            main.UniformNum = 10;
            main.MakeSD.Execute(main);
            new_event_reporter.Event.Should().BeTrue();
        }
    }

    //_____________________________________________________PLOT CREATION CLASSES TESTS_____________________________________________________
    public class ChartDataTest
    {
        //.................................Test of Data correctly receiving data from MainViewModel.MeasuredData
        [Theory]
        [InlineData(20, 10, 125, SPf.CubPol)]
        [InlineData(4, -10, 10, SPf.CubPol)]
        [InlineData(10, 0, 2, SPf.CubPol)]
        [InlineData(20, 10, 125, SPf.Exp)]
        [InlineData(4, -10, 10, SPf.Exp)]
        [InlineData(10, 0, 2, SPf.Exp)]
        public void MeasuredDataTest(int n, double min, double max, SPf func)
        {
            var err = new TestErrorReporter();
            var main = new MainViewModel(err);

            main.NonUniformNum = n;
            main.Min = min; main.Max = max;
            main.Function = func;
            main.MeasuredData = new MeasuredData(main.NonUniformNum, main.Min, main.Max, main.Function);

            Data data = new Data();
            data.AddMeasuredData(main.NonUniformNum, main.MeasuredData.NodeArray, main.MeasuredData.ValueArray);
            data.X.Length.Should().Be(main.NonUniformNum);
            data.Y.Length.Should().Be(main.NonUniformNum);

            for (int i = 0; i < main.NonUniformNum; i++)
            {
                data.X[i].Should().Be(main.MeasuredData.NodeArray[i]);
                data.Y[i].Should().Be(main.MeasuredData.ValueArray[i]);
            }
        }

        //.................................Test of Data correctly receiving data from MainViewModel.SplinesData
        [Theory]
        [InlineData(20, 10, 10, 125, SPf.CubPol, 1, 1, 0, 0)]
        [InlineData(20, 10, 10, 125, SPf.Exp, 1, 1, 0, 0)]
        //[InlineData(5, 10, 0, 2, SPf.CubPol, 1, 2, 3, 4)]
        //[InlineData(5, 10, 0, 2, SPf.Exp, 1, 2, 3, 4)]
        public void SplinesDataTest(
            int non_uniform_num, 
            int uniform_num, 
            double min, 
            double max, 
            SPf func, 
            double der_left_1, 
            double der_right_1, 
            double der_left_2, 
            double der_right_2
        ) {
            var err = new TestErrorReporter();
            var main = new MainViewModel(err);
            
            main.NonUniformNum = non_uniform_num;
            main.UniformNum = uniform_num;
            main.Min = min;
            main.Max = max;
            main.Function = func;
            main.Der1Left = der_left_1;
            main.Der1Right = der_right_1;
            main.Der2Left = der_left_2;
            main.Der2Right = der_right_2;
            
            main.MeasuredData = new MeasuredData(main.NonUniformNum, main.Min, main.Max, main.Function);
            main.SplineParameters = new SplineParameters(
                main.UniformNum, 
                main.Min, 
                main.Max, 
                main.Der1Left, 
                main.Der1Right, 
                main.Der2Left, 
                main.Der2Right
            );
            main.SplinesData = new SplinesData(main.MeasuredData, main.SplineParameters);
            main.SplinesData.BuildSpline();

            Data data = new Data();
            data.Splines_Y_List.Count.Should().Be(0);

            //.................................First spline test
            data.AddSplinesData(
                main.UniformNum,
                new double[2] {
                    main.Min, 
                    main.Max 
                },
                main.SplinesData.Spline1ValueArray,
                true
            );
            data.Splines_X.Length.Should().Be(main.UniformNum);
            data.Splines_Y_List.Count.Should().Be(1);

            double step = (main.Max - main.Min) / (main.UniformNum - 1);
            for (int i = 0; i < main.UniformNum; i++)
            {
                data.Splines_X[i].Should().Be(main.Min + i * step);
                data.Splines_Y_List[0][i].Should().Be(main.SplinesData.Spline1ValueArray[i]);
            }

            //.................................Second spline test
            data.AddSplinesData(
                main.UniformNum, 
                new double[2] { 
                    main.Min, 
                    main.Max 
                }, 
                main.SplinesData.Spline2ValueArray, 
                false
            );
            data.Splines_X.Length.Should().Be(main.UniformNum);
            data.Splines_Y_List.Count.Should().Be(2);

            for (int i = 0; i < main.UniformNum; i++)
            {
                data.Splines_X[i].Should().Be(main.Min + i * step);
                data.Splines_Y_List[1][i].Should().Be(main.SplinesData.Spline2ValueArray[i]);
            }
        }

        [Theory]
        [InlineData(20, 10, 125, SPf.CubPol)]
        [InlineData(4, -10, 10, SPf.CubPol)]
        [InlineData(10, 0, 2, SPf.CubPol)]
        [InlineData(20, 10, 125, SPf.Exp)]
        [InlineData(4, -10, 10, SPf.Exp)]
        [InlineData(10, 0, 2, SPf.Exp)]

        //.................................Test of ChartData correctly receiving data from MainViewModel.MeasuredData
        public void DataChartTest(int n, double min, double max, SPf func)
        {
            var err = new TestErrorReporter();
            var main = new MainViewModel(err);

            main.NonUniformNum = n;
            main.Min = min; main.Max = max;
            main.Function = func;
            main.MeasuredData = new MeasuredData(main.NonUniformNum, main.Min, main.Max, main.Function);

            Data data = new Data();
            data.AddMeasuredData(main.NonUniformNum, main.MeasuredData.NodeArray, main.MeasuredData.ValueArray);

            ChartData chart = new ChartData();
            chart.AddDataSeries(data);
            LineSeries testline = chart.plotModel.Series[0] as LineSeries;
            
            testline.Color.Should().Be(OxyColors.Green);
            testline.MarkerType.Should().Be(MarkerType.Circle);
            testline.MarkerSize.Should().Be(4);
            testline.MarkerStroke.Should().Be(OxyColors.Green);
            testline.MarkerFill.Should().Be(OxyColors.Green);

            for (int i = 0; i < main.NonUniformNum; i++)
            {
                testline.Points[i].X.Should().Be(main.MeasuredData.NodeArray[i]);
                testline.Points[i].Y.Should().Be(main.MeasuredData.ValueArray[i]);
            }    
        }

        //.................................Test of ChartData correctly receiving data from MainViewModel.SplinesData
        [Theory]
        [InlineData(20, 10, 10, 125, SPf.CubPol, 1, 1, 0, 0)]
        [InlineData(20, 10, 10, 125, SPf.Exp, 1, 1, 0, 0)]
        //[InlineData(5, 10, 0, 2, SPf.CubPol, 1, 2, 3, 4)]
        //[InlineData(5, 10, 0, 2, SPf.Exp, 1, 2, 3, 4)]
        public void SplinesChartTest(
            int non_uniform_num,
            int uniform_num,
            double min,
            double max,
            SPf func,
            double der_left_1,
            double der_right_1,
            double der_left_2,
            double der_right_2
        ) {
            var err = new TestErrorReporter();
            var main = new MainViewModel(err);

            main.NonUniformNum = non_uniform_num;
            main.UniformNum = uniform_num;
            main.Min = min; 
            main.Max = max;
            main.Function = func;
            main.Der1Left = der_left_1;
            main.Der1Right = der_right_1;
            main.Der2Left = der_left_2;
            main.Der2Right = der_right_2;

            main.MeasuredData = new MeasuredData(main.NonUniformNum, main.Min, main.Max, main.Function);
            main.SplineParameters = new SplineParameters(
                main.UniformNum,
                main.Min,
                main.Max,
                main.Der1Left,
                main.Der1Right,
                main.Der2Left,
                main.Der2Right
            );
            main.SplinesData = new SplinesData(main.MeasuredData, main.SplineParameters);
            main.SplinesData.BuildSpline();

            Data data = new Data();
            data.AddMeasuredData(main.NonUniformNum, main.MeasuredData.NodeArray, main.MeasuredData.ValueArray);
            data.AddSplinesData(
                main.UniformNum,
                new double[2] {
                    main.Min,
                    main.Max
                },
                main.SplinesData.Spline1ValueArray,
                true
            );
            data.AddSplinesData(
                main.UniformNum,
                new double[2] {
                    main.Min,
                    main.Max
                },
                main.SplinesData.Spline2ValueArray,
                false
            );

            ChartData chart = new ChartData();
            chart.AddDataSeries(data);
            chart.AddSplineSeries(data);

            //.................................First spline test
            LineSeries testline = chart.plotModel.Series[1] as LineSeries;
            testline.Color.Should().Be(OxyColors.Blue);
            testline.MarkerType.Should().Be(MarkerType.Circle);
            testline.MarkerSize.Should().Be(4);
            testline.MarkerStroke.Should().Be(OxyColors.Blue);
            testline.MarkerFill.Should().Be(OxyColors.Blue);

            double step = (main.Max - main.Min) / (main.UniformNum - 1);
            for (int i = 0; i < main.UniformNum; i++)
            {
                testline.Points[i].X.Should().Be(main.Min + i * step);
                testline.Points[i].Y.Should().Be(main.SplinesData.Spline1ValueArray[i]);
            }

            //.................................Second spline test
            testline = chart.plotModel.Series[2] as LineSeries;
            testline.Color.Should().Be(OxyColors.Orange);
            testline.MarkerType.Should().Be(MarkerType.Circle);
            testline.MarkerSize.Should().Be(4);
            testline.MarkerStroke.Should().Be(OxyColors.Orange);
            testline.MarkerFill.Should().Be(OxyColors.Orange);

            for (int i = 0; i < main.UniformNum; i++)
            {
                testline.Points[i].X.Should().Be(main.Min + i * step);
                testline.Points[i].Y.Should().Be(main.SplinesData.Spline2ValueArray[i]);
            }
        }
    }
}