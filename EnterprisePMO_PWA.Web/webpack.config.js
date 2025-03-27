// webpack.config.js
/**
 * To use this configuration:
 * 1. Install the required packages:
 *    npm install --save-dev webpack webpack-cli babel-loader @babel/core @babel/preset-env @babel/preset-react @babel/plugin-proposal-class-properties
 *    npm install --save react react-dom recharts lucide-react prop-types
 * 
 * 2. Add the following script to your package.json:
 *    "scripts": {
 *      "build": "webpack --config webpack.config.js",
 *      "watch": "webpack --config webpack.config.js --watch"
 *    }
 * 
 * 3. Run with: npm run build
 */

const path = require('path');

module.exports = {
  entry: {
    'project-components-bundle': './wwwroot/js/components/index.js',
  },
  output: {
    filename: '[name].js',
    path: path.resolve(__dirname, 'wwwroot/js/components'),
    library: 'EnterprisePMO',
    libraryTarget: 'umd',
  },
  mode: 'production',
  module: {
    rules: [
      {
        test: /\.(js|jsx)$/,
        exclude: /node_modules/,
        use: {
          loader: 'babel-loader',
          options: {
            presets: ['@babel/preset-env', '@babel/preset-react'],
            plugins: ['@babel/plugin-proposal-class-properties']
          }
        }
      }
    ]
  },
  resolve: {
    extensions: ['.js', '.jsx'],
    alias: {
      'react': path.resolve(__dirname, 'node_modules/react'),
      'react-dom': path.resolve(__dirname, 'node_modules/react-dom'),
      'recharts': path.resolve(__dirname, 'node_modules/recharts'),
      'lucide-react': path.resolve(__dirname, 'node_modules/lucide-react'),
    }
  },
  externals: {
    'react': 'React',
    'react-dom': 'ReactDOM',
    'prop-types': 'PropTypes',
    'recharts': 'Recharts',
    'lucide-react': 'lucide',
  }
};