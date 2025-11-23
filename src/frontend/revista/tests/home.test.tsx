// @ts-nocheck
import { render } from '@testing-library/react';
import Home from '../src/app/page';

test('Home renders', () => {
  const { getByText } = render(<Home /> as any);
  expect(getByText(/Revista Brasileira de Educação Básica/i)).toBeTruthy();
});
