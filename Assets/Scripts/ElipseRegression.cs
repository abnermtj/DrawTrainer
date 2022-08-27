//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Meta.Numerics.Matrices;
//using Meta.Numerics;

//namespace FitEllipse
//{
//    public class ElipseRegression
//    {
//        public RectangularMatrix Fit(PointCollection points)
//        {
//            int numPoints = points.Count;

//            RectangularMatrix D1 = new RectangularMatrix(numPoints, 3);
//            RectangularMatrix D2 = new RectangularMatrix(numPoints, 3);
//            SquareMatrix S1 = new SquareMatrix(3);
//            SquareMatrix S2 = new SquareMatrix(3);
//            SquareMatrix S3 = new SquareMatrix(3);
//            SquareMatrix T = new SquareMatrix(3);
//            SquareMatrix M = new SquareMatrix(3);
//            SquareMatrix C1 = new SquareMatrix(3);
//            RectangularMatrix a1 = new RectangularMatrix(3, 1);
//            RectangularMatrix a2 = new RectangularMatrix(3, 1);
//            RectangularMatrix result = new RectangularMatrix(6, 1);
//            RectangularMatrix temp;

//            C1[0, 0] = 0;
//            C1[0, 1] = 0;
//            C1[0, 2] = 0.5;
//            C1[1, 0] = 0;
//            C1[1, 1] = -1;
//            C1[1, 2] = 0;
//            C1[2, 0] = 0.5;
//            C1[2, 1] = 0;
//            C1[2, 2] = 0;

//            //2 D1 = [x .ˆ 2, x .* y, y .ˆ 2]; % quadratic part of the design matrix
//            //3 D2 = [x, y, ones(size(x))]; % linear part of the design matrix
//            for (int xx = 0; xx < points.Count; xx++)
//            {
//                Point p = points[xx];
//                D1[xx, 0] = p.X * p.X;
//                D1[xx, 1] = p.X * p.Y;
//                D1[xx, 2] = p.Y * p.Y;

//                D2[xx, 0] = p.X;
//                D2[xx, 1] = p.Y;
//                D2[xx, 2] = 1;
//            }

//            //4 S1 = D1’ * D1; % quadratic part of the scatter matrix
//            temp = D1.Transpose * D1;
//            for (int xx = 0; xx < 3; xx++)
//                for (int yy = 0; yy < 3; yy++)
//                    S1[xx, yy] = temp[xx, yy];

//            //5 S2 = D1’ * D2; % combined part of the scatter matrix
//            temp = D1.Transpose * D2;
//            for (int xx = 0; xx < 3; xx++)
//                for (int yy = 0; yy < 3; yy++)
//                    S2[xx, yy] = temp[xx, yy];

//            //6 S3 = D2’ * D2; % linear part of the scatter matrix
//            temp = D2.Transpose * D2;
//            for (int xx = 0; xx < 3; xx++)
//                for (int yy = 0; yy < 3; yy++)
//                    S3[xx, yy] = temp[xx, yy];

//            //7 T = - inv(S3) * S2’; % for getting a2 from a1
//            T = -1 * S3.Inverse() * S2.Transpose;

//            //8 M = S1 + S2 * T; % reduced scatter matrix
//            M = S1 + S2 * T;

//            //9 M = [M(3, <img draggable="false" role="img" class="emoji" alt="🙂" src="https://s0.wp.com/wp-content/mu-plugins/wpcom-smileys/twemoji/2/svg/1f642.svg" scale="0"> ./ 2; - M(2, :); M(1, <img draggable="false" role="img" class="emoji" alt="🙂" src="https://s0.wp.com/wp-content/mu-plugins/wpcom-smileys/twemoji/2/svg/1f642.svg" scale="0"> ./ 2]; % premultiply by inv(C1)
//            M = C1 * M;

//            //10 [evec, eval] = eig(M); % solve eigensystem
//            ComplexEigensystem eigenSystem = M.Eigensystem();

//            //11 cond = 4 * evec(1, <img draggable="false" role="img" class="emoji" alt="🙂" src="https://s0.wp.com/wp-content/mu-plugins/wpcom-smileys/twemoji/2/svg/1f642.svg" scale="0"> .* evec(3, <img draggable="false" role="img" class="emoji" alt="🙂" src="https://s0.wp.com/wp-content/mu-plugins/wpcom-smileys/twemoji/2/svg/1f642.svg" scale="0"> - evec(2, <img draggable="false" role="img" class="emoji" alt="🙂" src="https://s0.wp.com/wp-content/mu-plugins/wpcom-smileys/twemoji/2/svg/1f642.svg" scale="0"> .ˆ 2; % evaluate a’Ca
//            //12 a1 = evec(:, find(cond > 0)); % eigenvector for min. pos. eigenvalue
//            for (int xx = 0; xx < eigenSystem.Dimension; xx++)
//            {
//                Vector<Complex> vector = eigenSystem.Eigenvector(xx);
//                Complex condition = 4 * vector[0] * vector[2] - vector[1] * vector[1];
//                if (condition.Im == 0 && condition.Re > 0)
//                {
//                    // Solution is found
//                    Console.WriteLine("\nSolution Found!");
//                    for (int yy = 0; yy < vector.Count(); yy++)
//                    {
//                        Console.Write("{0}, ", vector[yy]);
//                        a1[yy, 0] = vector[yy].Re;
//                    }
//                }
//            }
//            //13 a2 = T * a1; % ellipse coefficients
//            a2 = T * a1;

//            //14 a = [a1; a2]; % ellipse coefficients
//            result[0, 0] = a1[0, 0];
//            result[1, 0] = a1[1, 0];
//            result[2, 0] = a1[2, 0];
//            result[3, 0] = a2[0, 0];
//            result[4, 0] = a2[1, 0];
//            result[5, 0] = a2[2, 0];

//            return result;
//        }
//    }
//}