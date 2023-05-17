namespace MoogleEngine;

public class Matrix
{
    private double[,] Coefs;
    public int Cols {get; private set;}
    public int Rows {get; private set;}

    public int GetLength(int i)
    {
        switch(i)
        {
            case 0: return Rows;
            case 1: return Cols;
            default: throw new ArgumentException("Invalid dimension");
        }
    }

    public double this[int r, int c]
    {
        get
        {
            if (!ValidPos(r,c)) throw new IndexOutOfRangeException();
            return Coefs[r, c];
        }
        set
        {
            if (!ValidPos(r,c)) throw new IndexOutOfRangeException();
            Coefs[r, c] = value;
        }
    }
    public Matrix(double[,] coefs)
    {
        this.Coefs = (double[,])coefs.Clone();
        this.Rows = this.Coefs.GetLength(0);
        this.Cols = this.Coefs.GetLength(1);
    }

    public Matrix(int SquareDimension)
    {
        this.Coefs = new double[SquareDimension, SquareDimension];
        // for (int i = 0; i < Rows; i++)
        //     for (int j = 0; j < Cols; j++)
        //         this[i, j] = 0;

        this.Cols = SquareDimension;
        this.Rows = this.Cols;
    }

    public Matrix(int Rows, int Cols)
    {
        this.Coefs = new double[Rows, Cols];
        // for (int i = 0; i < Rows; i++)
        //     for (int j = 0; j < Cols; j++)
        //         this[i, j] = 0;
        
        this.Rows = Rows;
        this.Cols = Cols;
    }

    public Matrix(Matrix other)
    {
        this.Coefs = (double[,])other.Coefs.Clone();
        this.Cols = other.Cols;
        this.Rows = other.Rows;
    }

    public static Matrix GetMatrix()
    {
        int rows;
        while (true)
        {
            rows = Getters.GetInt("Input Row Number: ");
            if (rows <= 0) System.Console.WriteLine("Row Number Can't be <= 0\n");
            else break;
        }
        int cols;
        while (true)
        {
            cols = Getters.GetInt("Input Column number: ");
            if (cols <= 0) System.Console.WriteLine("Column Number Can't be <= 0\n");
            else break;
        }

        var answ = new Matrix(rows, cols);

        for (int i = 0; i < answ.Rows; i++)
        {
            for (int j = 0; j < answ.Cols; j++)
            {
                answ[i, j] = Getters.GetDouble($"Input Element {i},{j}: ");
            }
        }

        return answ;
    }

    public static Matrix IdentityMatrix(int dimension)
    {
        var answ = new Matrix(dimension);
        for (int i = 0; i < answ.Rows; i++)
        {
            answ[i,i] = 1;
        }

        return answ;
    }

    public bool IsSquare() => this.Rows == this.Cols;

    public static Matrix operator*(Matrix a, Matrix b)
    {
        if (a.Cols != b.Rows) 
            throw new ArgumentException
            (
                $"Can't multiply {a.Rows}x{a.Cols} matrix with {b.Rows}x{b.Cols} matrix"
            );   
    
        var answ = new Matrix(a.Rows, b.Cols);
        
        for (int answ_row = 0; answ_row < a.Rows; answ_row++)
        {
            for (int answ_col = 0; answ_col < b.Cols; answ_col++)
            {
                double sum = 0;
                for (int k = 0; k < a.Cols; k++)
                {
                    sum += a[answ_row, k]*b[k, answ_col];
                }

                answ[answ_row, answ_col] = sum;
            }
        }

        return answ;
    }

    public static Matrix operator+(Matrix a, Matrix b)
    {
        if ((a.Rows != b.Rows) || (a.Cols != b.Cols))
            throw new ArgumentException
            (
                $"Can't add {a.Rows}x{a.Cols} matrix with {b.Rows}x{b.Cols} matrix"
            );

        var answ = new Matrix(a);

        for (int i = 0; i < b.Rows; i++)
        {
            for (int j = 0; j < b.Cols; j++)
            {
                answ[i, j] += b[i, j];
            }
        }

        return answ;
    }

    public static Matrix operator*(double e, Matrix a)
    {
        var answ = new Matrix(a);

        for (int i = 0; i < a.Rows; i++)
        {
            for (int j = 0; j < a.Cols; j++)
            {
                answ[i, j] *= e;
            }
        }

        return answ;
    }

    public static Matrix operator-(Matrix a, Matrix b) => a + (-1)*b; // yes, lazy

    public static Matrix operator*(Matrix a, double e) => e*a;
    public static Matrix operator^(Matrix a, uint n)
    {
        if (a.Cols != a.Rows) throw new ArgumentException
        (
            $"Can't multiply {a.Rows}x{a.Cols} matrix with {a.Rows}x{a.Cols} matrix"
        );
        
        if (n == 0) return IdentityMatrix(a.Cols);
        
        var answ = IdentityMatrix(a.Cols);

        for (int i = 0; i < n; i++)
        {
            answ *= a;
        }

        return answ;
    }

    private bool ValidPos(int i, int j)
    {
        return (i >= 0 && i < this.Rows &&
                j >= 0 && j < this.Cols);
    }

    public override string ToString()
    {
        // find the longest number
        int longest = 0;
        for (int i = 0; i < this.Rows; i++)
        {
            for (int j = 0; j < this.Cols; j++)
            {
                if (this[i,j].ToString().Length > longest)
                {
                    longest = this[i,j].ToString().Length;
                }
            }
        }

        var answ = new List<char>(this.Rows*this.Cols*longest);

        for (int i = 0; i < this.Rows; i++)
        {
            for (int j = 0; j < this.Cols; j++)
            {
                foreach(var character in ($"{this[i,j]}".PadRight(longest+2)))
                {
                    answ.Add(character);
                }                
            }
            if (i < this.Rows -1) answ.Add('\n');
        }

        return new string(answ.ToArray()); 
               
    }

    public Matrix Transpose()
    {
        var answ = new Matrix(this.Cols, this.Rows);

        for (int i = 0; i < this.Rows; i++)
        {
            for (int j = 0; j < this.Cols; j++)
            {
                answ[j, i] = this[i, j];
            }
        }

        return answ;
    }

    public Matrix Inverse()
    {
        if (!this.IsSquare()) throw new ArithmeticException("Non-square Matrices are not inversible");
        
        var det = this.Determinant();
        if (det == 0) throw new ArithmeticException("Determinant must be non-0 for matrix to be inversible");

        var cofactorMatrix = new Matrix(this.Cols);
        for (int i = 0; i < cofactorMatrix.Rows; i++)
        {
            for (int j = 0; j < cofactorMatrix.Cols; j++)
            {
                var cofactor = this.SupressRowCol(i, j).Determinant();
                if ((i + j) % 2 != 0) cofactor *= -1;
                cofactorMatrix[i, j] = cofactor/det;
            }
        }

        return cofactorMatrix.Transpose();
    }


    private Matrix SupressRowCol(int row, int col)
    {
        
        if (!ValidPos(row, col)) throw new IndexOutOfRangeException();

        var answ = new Matrix(this.Rows - 1, this.Cols - 1);

        for (int i = 0, answI = 0; i < this.Rows; i++)
        {
            if (i == row) continue;
            for (int j = 0, answJ = 0; j < this.Cols; j++)
            {
                if (j == col) continue;
                answ[answI,answJ] = this[i,j];
                answJ++;
            }
            answI++;
        }

        return answ;
    }

    public double Determinant()
    {
        if (!this.IsSquare()) throw new ArithmeticException("Can't find Determinant of non-square Matrices");
        if (this.Rows == 1) return this[0,0];
        if (this.Rows == 2) return(this[0,0]*this[1,1] - this[0,1]*this[1,0]);

        double answ = 0;
        for (int i = 0; i < this.Cols; i++)
        {
            if (this[0,i] == 0) continue;
            if (i % 2 == 0)     answ += this[0,i]*this.SupressRowCol(0, i).Determinant();
            else                answ -= this[0,i]*this.SupressRowCol(0, i).Determinant();
        }

        return answ;
    }

    
}

public class Getters
{
    public static int GetInt(string message = "")
    {
        int answ = 0;
        bool gotten;
        do
        {
            gotten = true;

            if (message != "") Console.Write(message);
            var input = Console.ReadLine();
            try
            {
               answ = int.Parse(input); 
            }
            catch (FormatException)
            {
                Console.WriteLine("\n *** INCORRECT FORMAT *** \n");
                gotten = false;
            }
            catch (ArgumentNullException)
            {
                System.Console.WriteLine("\n *** NO INPUT *** \n");
                gotten = false;
            }

        } while(!gotten);
        
        return answ;
    }

    public static double GetDouble(string message = "")
    {
        double answ = 0;
        bool gotten;
        do
        {
            gotten = true;

            if (message != "") Console.Write(message);
            var input = Console.ReadLine();
            try
            {
               answ = double.Parse(input); 
            }
            catch (FormatException)
            {
                Console.WriteLine("\n *** INCORRECT FORMAT *** \n");
                gotten = false;
            }
            catch (ArgumentNullException)
            {
                System.Console.WriteLine("\n *** NO INPUT *** \n");
                gotten = false;
            }

        } while(!gotten);
        
        return answ;
    }
}

