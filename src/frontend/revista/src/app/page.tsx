import type { Metadata } from "next";
import HomeClient from "./HomeClient"

export const metadata: Metadata = {
  title: 'Revista Brasileira de Educação Básica',
  description: 'Home da RBEB',
};

export default function Home() {
  return <HomeClient />;
}