'use client';

import { VersaoArtigo } from '@/types/enums';

interface ProgressBarProps {
    currentVersion: VersaoArtigo;
}


const versionConfig = {
    [VersaoArtigo.Original]: {
        label: 'Original',
        color: 'bg-pink-300',
        border: 'border-pink-400',
        filled: 1
    },
    [VersaoArtigo.PrimeiraEdicao]: {
        label: '1ª Edição',
        color: 'bg-yellow-300',
        border: 'border-yellow-400',
        filled: 2
    },
    [VersaoArtigo.SegundaEdicao]: {
        label: '2ª Edição',
        color: 'bg-green-300',
        border: 'border-green-500',
        filled: 3
    },
    [VersaoArtigo.TerceiraEdicao]: {
        label: '3ª Edição',
        color: 'bg-blue-300',
        border: 'border-blue-500',
        filled: 4
    },
    [VersaoArtigo.QuartaEdicao]: {
        label: '4ª Edição',
        color: 'bg-blue-500',
        border: 'border-blue-700',
        filled: 5
    },
    [VersaoArtigo.Final]: {
        label: 'Final',
        color: 'bg-blue-700',
        border: 'border-blue-900',
        filled: 5
    },
};

export default function ProgressBar({ currentVersion }: ProgressBarProps) {
    // Pega a configuração da versão atual, ou 'Original' se for inválida
    const config = versionConfig[currentVersion] || versionConfig[VersaoArtigo.Original];

    // Converte o enum numérico (0-5) para uma porcentagem (0-100)
    // Usa a equação para garantir o valor correto (Versao / 5) * 100%
    const ballPositionPercent = (Number(currentVersion) / 5) * 100;

    // Array para os 5 segmentos da barra (0 é o de baixo, 4 é o de cima)
    const segments = [0, 1, 2, 3, 4];

    return (
        <div
            className="fixed top-1/2 -translate-y-1/2 flex flex-col items-center"
            style={{
                left: '1%',
                height: '70vh', // 70% da altura da tela
                zIndex: 50 // Garante que flutue sobre outros elementos
            }}
        >
            {/* A Barra (Termômetro) */}
            <div
                className="relative bg-gray-200/40"
                style={{
                    width: '10px',
                    height: '100%',
                    borderWidth: '3px',
                    borderColor: 'rgba(0, 0, 0, 0.4)',
                    // Sombras 
                    boxShadow: '0 6px 10px rgba(0,0,0,0.4), inset 4px 0 6px rgba(0,0,0,0.35)',
                }}
            >
                {/* Bola Branca */}
                <div
                    className="absolute left-1/2 -translate-x-1/2 w-[10px] h-[10px] bg-white rounded-full border border-gray-400"
                    style={{
                        // Posição 0% é o fundo, 100% é o topo
                        // Subtrai 5px (metade da altura da bola) para centralizá-la
                        bottom: `calc(${ballPositionPercent}% - 5px)`,
                        zIndex: 2,
                        transition: 'bottom 0.5s ease-out' // Animação suave
                    }}
                />

                {/* Texto da Versão (Abaixo da Bola) */}
                <div
                    className="absolute left-1/2 -translate-x-1/2 text-center"
                    style={{
                        // Posição 30px abaixo da bola
                        bottom: `calc(${ballPositionPercent}% - 30px)`,
                        width: '100px', // Largura para o texto não quebrar
                        zIndex: 2,
                        transition: 'bottom 0.5s ease-out'
                    }}
                >
                    <span className="text-xs font-semibold text-gray-700">{config.label}</span>
                </div>

                {/* Divs Internos (Empilhados de baixo para cima) */}
                <div className="absolute inset-0 flex flex-col-reverse">
                    {segments.map(index => (
                        <div
                            key={index}
                            className={`h-[20%] w-full ${
                                // Colore o segmento se o índice for MENOR que o número de 'filled'
                                index < config.filled
                                    ? `${config.color} ${config.border}`
                                    : 'border-transparent'
                                }`}
                            style={{
                                // Adiciona a borda superior (exceto para o primeiro)
                                borderTopWidth: index > 0 ? '1px' : '0',
                                transition: 'background-color 0.5s ease'
                            }}
                        />
                    ))}
                </div>
            </div>
        </div>
    );
}