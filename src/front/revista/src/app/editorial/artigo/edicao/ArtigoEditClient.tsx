'use client';

import { useState, useEffect, Suspense, useMemo } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useQuery, useMutation, ApolloError } from '@apollo/client';
import {
    OBTER_ARTIGO_EDITORIAL_VIEW,
    OBTER_STAFF_LIST,
    ATUALIZAR_CONTEUDO_ARTIGO,
    ATUALIZAR_METADADOS_ARTIGO,
    ADD_STAFF_COMENTARIO,
} from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import ProgressBar from '@/components/ProgressBar';
import StaffCommentCard from '@/components/StaffCommentCard';
import { StaffMember } from '@/components/StaffCard';
import { PosicaoEditorial, VersaoArtigo } from '@/types/enums';
import { StaffComentario } from '@/types/index';
import toast from 'react-hot-toast';
import Image from 'next/image';
import { User, Send } from 'lucide-react';
import StaffControlBar from '@/components/StaffControlBar';
import CreateCommentCard from '@/components/CreateCommentCard';
import EditorialQuillEditor from '@/components/EditorialQuillEditor';
import type { Range } from 'quill';

// --- Tipos ---

interface EditorialTeamData {
    initialAuthorId: string[];
    editorId: string;
    reviewerIds: string[];
    correctorIds: string[];
    __typename: "EditorialTeam";
}

export interface ArtigoData {
    id: string;
    titulo: string;
    resumo: string;
    tipo: any;
    status: any;
    permitirComentario: boolean;
    editorialId: string;
    editorial: {
        position: PosicaoEditorial;
        team: EditorialTeamData;
        __typename: "EditorialView";
    };
}

interface EditorialViewData {
    obterArtigoEditorialView: ArtigoData & {
        autorIds: string[];
        editorial: {
            currentHistoryId: string;
        };
        conteudoAtual: {
            version: VersaoArtigo;
            content: string;
            midias: {
                idMidia: string;
                url: string;
                textoAlternativo: string;
            }[];
            staffComentarios: StaffComentario[];
            __typename: "ArtigoHistoryEditorialView";
        };
        interacoes: {
            comentariosEditoriais: any[];
            __typename: "InteractionConnectionDTO";
        };
        __typename: "ArtigoEditorialView";
    };
}

interface StaffQueryData {
    obterStaffList: StaffMember[];
}

// --- Componente de Equipe Editorial (Topo) ---
const TeamHeader = ({ team, staffList }: { team: EditorialTeamData, staffList: StaffMember[] }) => {

    const allTeamIds = [
        ...team.initialAuthorId,
        ...team.reviewerIds,
        ...team.correctorIds,
        team.editorId
    ].filter(Boolean);

    const teamMembers = allTeamIds
        .map(id => staffList.find(s => s?.usuarioId === id))
        .filter((s): s is StaffMember => !!s);

    return (
        <div className="flex flex-wrap gap-x-12 gap-y-4 mb-6 px-2 py-4 border-b border-gray-200">
            {teamMembers.map(member => (
                <div key={member.usuarioId} className="group relative flex flex-col items-center w-[50px]">
                    <div
                        className="relative h-[25px] w-[25px] rounded-full overflow-hidden transition-all duration-300 opacity-40 group-hover:opacity-100 group-hover:rounded-none group-hover:h-[50px] group-hover:w-[50px]"
                    >
                        {member.url ? (
                            <Image src={member.url} alt={member.nome} fill className="object-cover" />
                        ) : (
                            <User className="w-full h-full text-gray-500 bg-gray-200 p-1" />
                        )}
                    </div>
                    <div className="absolute top-full mt-2 hidden group-hover:block p-2 bg-black/80 text-white rounded-md text-center shadow-lg z-20">
                        <p className="text-xs font-bold whitespace-nowrap">{member.nome}</p>
                        <p className="text-xs text-gray-300 whitespace-nowrap">{member.job}</p>
                    </div>
                </div>
            ))}
        </div>
    );
};


// --- Componente Principal ---
function ArtigoEditClient() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const artigoId = searchParams.get('Id');

    const { user, logout } = useAuth();
    const [isStaff, setIsStaff] = useState(false);
    const [userRole, setUserRole] = useState<'staff' | 'author' | 'team' | 'none'>('none');

    const [activeStaffComment, setActiveStaffComment] = useState<StaffComentario | null>(null);
    const [selectedQuillRange, setSelectedQuillRange] = useState<Range | null>(null);
    const [newStaffComment, setNewStaffComment] = useState('');

    const [editTitle, setEditTitle] = useState('');
    const [editResumo, setEditResumo] = useState('');
    const [editContent, setEditContent] = useState('');
    const [editMidias, setEditMidias] = useState<any[]>([]);

    // --- Query Verificação de Staff e Lista de Staff ---
    const { data: staffData, loading: loadingStaff } = useQuery<StaffQueryData>(OBTER_STAFF_LIST, {
        variables: { page: 0, pageSize: 200 },
        fetchPolicy: 'cache-and-network',
        onError: (err: ApolloError) => {
            if (err.graphQLErrors.some(e => e.extensions?.code === 'AUTH_FORBIDDEN')) {
                localStorage.removeItem('isStaff');
                logout();
                router.push('/');
            }
        },
        onCompleted: (data) => {
            const staffMember = data.obterStaffList.find(s => s?.usuarioId === user?.id);
            if (staffMember && staffMember.isActive) {
                setIsStaff(true);
            }
        }
    });

    const staffList = staffData?.obterStaffList?.filter((s): s is StaffMember => !!s) ?? [];

    // --- Query Dados Principais do Artigo ---
    const { data, loading, error, refetch } = useQuery<EditorialViewData>(OBTER_ARTIGO_EDITORIAL_VIEW, {
        variables: { artigoId },
        skip: !artigoId || !user,
        fetchPolicy: 'network-only',
        onCompleted: (data) => {
            const conteudo = data.obterArtigoEditorialView.conteudoAtual;
            setEditTitle(data.obterArtigoEditorialView.titulo);
            setEditResumo(data.obterArtigoEditorialView.resumo);
            setEditContent(conteudo.content);
            setEditMidias(conteudo.midias.map(m => ({ midiaID: m.idMidia, url: m.url, alt: m.textoAlternativo })));
        },
        onError: (err: ApolloError) => {
            if (err.graphQLErrors.some(e => e.extensions?.code === 'AUTH_FORBIDDEN')) {
                toast.error("Acesso negado. Você não tem permissão para ver este artigo.");
                router.push('/');
            } else {
                toast.error(`Erro ao carregar artigo: ${err.message}`);
            }
        }
    });

    const artigo = data?.obterArtigoEditorialView;
    const editorial = artigo?.editorial;
    const conteudo = artigo?.conteudoAtual;

    // --- Mutações ---
    const [addStaffComment, { loading: loadingAddComment }] = useMutation(ADD_STAFF_COMENTARIO, {
        onCompleted: () => {
            toast.success('Comentário salvo!');
            setNewStaffComment('');
            setSelectedQuillRange(null);
            refetch();
        },
        onError: (err) => toast.error(`Erro ao salvar: ${err.message}`)
    });

    const [atualizarConteudo, { loading: loadingContent }] = useMutation(ATUALIZAR_CONTEUDO_ARTIGO);
    const [atualizarMetadados, { loading: loadingMeta }] = useMutation(ATUALIZAR_METADADOS_ARTIGO);

    // --- Lógica de Determinação de Visualização ---
    useEffect(() => {
        if (loadingStaff || loading || !artigo || !user) return;
        const team = editorial?.team;
        if (isStaff) { setUserRole('staff'); return; }
        if (team?.initialAuthorId.includes(user.id)) { setUserRole('author'); return; }
        if (team?.reviewerIds.includes(user.id) || team?.correctorIds.includes(user.id) || team?.editorId === user.id) {
            setUserRole('team');
            return;
        }
        toast.error("Você não tem permissão para acessar esta página.");
        router.push('/');
    }, [user, isStaff, artigo, loading, loadingStaff, router, editorial?.team]);

    // Lógica de Modo de Visualização (Edit vs Comment)
    const mode = useMemo((): 'edit' | 'comment' => {
        if (!artigo || !editorial || !conteudo) return 'comment';

        const isProntoParaPublicar = editorial.position === PosicaoEditorial.ProntoParaPublicar;
        const isVersaoFinal = conteudo.version >= VersaoArtigo.TerceiraEdicao;
        const isCorretorOuChefe = isStaff;

        if (isProntoParaPublicar && isVersaoFinal && isCorretorOuChefe) {
            return 'edit';
        }

        if (userRole === 'author') {
            if (isProntoParaPublicar) return 'comment';
            if (conteudo.staffComentarios.length > 0) return 'comment';
            return 'edit';
        }

        return 'comment';

    }, [userRole, artigo, editorial, conteudo, isStaff]);


    // --- Handlers de Ação ---

    const handleTextSelect = (range: Range) => {
        if (range.length > 0) {
            setSelectedQuillRange(range);
            setActiveStaffComment(null);
        } else {
            setSelectedQuillRange(null);
        }
    };

    const handleHighlightClick = (comment: StaffComentario) => {
        setActiveStaffComment(comment);
        setSelectedQuillRange(null);
    };

    const handleContentChange = (html: string) => {
        setEditContent(html);
    };

    const handleMediaChange = (midia: { id: string; url: string; alt: string }) => {
        console.log("Mídia alterada:", midia);
        setEditMidias(prev => [...prev, { midiaID: midia.id, url: midia.url, alt: midia.alt }]);
    };

    const handleCreateStaffComment = () => {
        if (!newStaffComment.trim() || !selectedQuillRange || !editorial) return;

        const commentData = {
            selection: selectedQuillRange,
            comment: newStaffComment,
            date: new Date().toISOString(),
            commentId: `temp-${Date.now()}`,
            parent: null,
        };

        addStaffComment({
            variables: {
                historyId: editorial.currentHistoryId,
                comment: JSON.stringify(commentData),
                parent: null
            }
        });
    };

    const handleSaveAuthorChanges = () => {
        if (!artigo || !conteudo) return;

        const toastId = toast.loading('Salvando alterações...');

        const p1 = atualizarConteudo({
            variables: {
                artigoId: artigo.id,
                newContent: editContent,
                midias: editMidias,
                commentary: "Conteúdo atualizado pelo autor/corretor"
            }
        });

        const p2 = atualizarMetadados({
            variables: {
                id: artigo.id,
                input: {
                    titulo: editTitle,
                    resumo: editResumo,
                    status: null,
                    tipo: null,
                    idsAutor: null,
                    referenciasAutor: null,
                    permitirComentario: null,
                    posicao: null
                },
                commentary: "Metadados atualizados pelo autor/corretor"
            }
        });

        Promise.all([p1, p2])
            .then(() => {
                toast.success('Artigo salvo com sucesso!', { id: toastId });
                refetch();
            })
            .catch((err) => {
                toast.error(`Erro ao salvar: ${err.message}`, { id: toastId });
            });
    };

    const handleCancelAuthorChanges = () => {
        if (!artigo || !conteudo) return;
        setEditTitle(artigo.titulo);
        setEditResumo(artigo.resumo);
        setEditContent(conteudo.content);
        setEditMidias(conteudo.midias.map(m => ({ midiaID: m.idMidia, url: m.url, alt: m.textoAlternativo })));
        toast('Alterações descartadas.', { icon: '🔄' });
    };

    // --- Renderização ---

    if (loading || loadingStaff || !artigo || !editorial || !conteudo) {
        return <Layout><p className="text-center mt-20">Carregando dados editoriais...</p></Layout>;
    }

    const jaPostouEditorial = artigo.interacoes.comentariosEditoriais.some(
        comment => comment.usuarioId === user?.id
    );

    return (
        <Layout>
            <ProgressBar currentVersion={conteudo.version} />

            <div
                className="relative"
                style={{
                    marginLeft: '40px',
                    padding: '1% 0.5%',
                    width: 'calc(100% - 40px)'
                }}
            >

                <TeamHeader
                    team={editorial.team}
                    staffList={staffList}
                />

                {userRole === 'staff' && (
                    <StaffControlBar
                        artigoId={artigo.id}
                        editorialId={artigo.editorialId}
                        currentData={artigo as ArtigoData}
                        staffList={staffList}
                        onUpdate={refetch}
                    />
                )}

                <div className="flex gap-4 mt-6">

                    <div
                        className="flex-1"
                        style={{ width: (conteudo.version > 0 || mode === 'comment') ? '80%' : '100%' }}
                    >
                        {mode === 'edit' ? (
                            <div className="space-y-4">
                                <div>
                                    <label className="block text-lg font-semibold text-gray-700 mb-2">Título</label>
                                    <input
                                        type="text"
                                        value={editTitle}
                                        onChange={(e) => setEditTitle(e.target.value)}
                                        className="w-full p-3 border border-gray-300 rounded-lg"
                                    />
                                </div>
                                <div>
                                    <label className="block text-lg font-semibold text-gray-700 mb-2">Resumo</label>
                                    <textarea
                                        value={editResumo}
                                        onChange={(e) => setEditResumo(e.target.value)}
                                        className="w-full p-3 border border-gray-300 rounded-lg h-32 resize-none"
                                    />
                                </div>
                                <EditorialQuillEditor
                                    mode="edit"
                                    initialContent={editContent}
                                    onContentChange={handleContentChange}
                                    onMediaChange={handleMediaChange}
                                />
                            </div>
                        ) : (
                            <div>
                                <h3 className="text-xl font-semibold mb-4">Modo de Comentário</h3>
                                <EditorialQuillEditor
                                    mode="comment"
                                    initialContent={conteudo.content}
                                    staffComments={conteudo.staffComentarios}
                                    onTextSelect={handleTextSelect}
                                    onHighlightClick={handleHighlightClick}
                                />
                            </div>
                        )}

                        {userRole !== 'staff' && mode === 'edit' && (
                            <div className="flex justify-end gap-4 mt-6">
                                <button
                                    onClick={handleCancelAuthorChanges}
                                    className="px-5 py-2 rounded-lg border border-gray-300"
                                >
                                    Cancelar
                                </button>
                                <button
                                    onClick={handleSaveAuthorChanges}
                                    disabled={loadingContent || loadingMeta}
                                    className="px-5 py-2 rounded-lg bg-emerald-600 text-white font-bold shadow disabled:bg-gray-400"
                                >
                                    {loadingContent || loadingMeta ? 'Salvando...' : 'Salvar Alterações'}
                                </button>
                            </div>
                        )}

                        {editorial.position === PosicaoEditorial.ProntoParaPublicar && (
                            <div className="mt-10 pt-6 border-t">
                                <h3 className="text-2xl font-semibold mb-4">Comentário Editorial</h3>
                                {jaPostouEditorial ? (
                                    <p className="text-gray-600 italic">Você já enviou seu comentário editorial para este artigo.</p>
                                ) : (
                                    <>
                                        <p className="text-sm text-gray-600 mb-4">
                                            Esta é a etapa final. Adicione um comentário editorial (público) que será exibido
                                            junto ao artigo quando ele for publicado. (Isto só pode ser feito uma vez).
                                        </p>
                                        <CreateCommentCard
                                            artigoId={artigo.id}
                                            onCommentPosted={refetch}
                                            isEditorial={true}
                                        />
                                    </>
                                )}
                            </div>
                        )}

                    </div>

                    {(mode === 'comment' || conteudo.version > 0) && (
                        <div style={{ width: '20%' }} className="flex-shrink-0">
                            <h4 className="text-lg font-semibold mb-4">Comentários da Equipe</h4>
                            <div className="space-y-3 max-h-[80vh] overflow-y-auto p-2 bg-gray-50 rounded-md">

                                {selectedQuillRange && (
                                    <div className="p-3 bg-white border border-emerald-300 rounded-lg shadow-md">
                                        <p className="text-sm font-semibold mb-2">Adicionar Comentário</p>
                                        <textarea
                                            value={newStaffComment}
                                            onChange={(e) => setNewStaffComment(e.target.value)}
                                            placeholder="Escreva seu comentário sobre o texto selecionado..."
                                            className="w-full h-24 p-2 text-sm border border-gray-300 rounded-md"
                                        />
                                        <div className="flex justify-end gap-2 mt-2">
                                            <button onClick={() => setSelectedQuillRange(null)} className="px-2 py-1 text-xs" disabled={loadingAddComment}>Cancelar</button>
                                            <button onClick={handleCreateStaffComment} className="px-2 py-1 text-xs bg-emerald-600 text-white rounded" disabled={loadingAddComment || !newStaffComment.trim()}>
                                                {loadingAddComment ? '...' : <Send size={14} />}
                                            </button>
                                        </div>
                                    </div>
                                )}

                                {activeStaffComment && (
                                    <StaffCommentCard
                                        key={activeStaffComment.id}
                                        comment={activeStaffComment}
                                        historyId={editorial.currentHistoryId}
                                        onClose={() => setActiveStaffComment(null)}
                                        onCommentChange={refetch}
                                        staffList={staffList}
                                    />
                                )}

                                <p className="text-xs text-gray-500 pt-4 border-t">
                                    Passe o mouse sobre os textos destacados no artigo para ver os comentários.
                                </p>

                                {conteudo.staffComentarios.filter(c => !c.parent).map(comment => (
                                    <div key={comment.id} className="p-2 border-b border-gray-200">
                                        <p
                                            className="text-xs text-gray-700 truncate cursor-pointer hover:underline"
                                            onClick={() => handleHighlightClick(comment)}
                                        >
                                            {/* Tenta fazer o parse do JSON para exibir o texto */}
                                            {(() => {
                                                try {
                                                    const data = JSON.parse(comment.comment);
                                                    return data.comment || "Comentário inválido";
                                                } catch {
                                                    return comment.comment;
                                                }
                                            })().substring(0, 100)}...
                                        </p>
                                    </div>
                                ))}

                            </div>
                        </div>
                    )}

                </div>

            </div>
        </Layout>
    );
}

export default function ArtigoEditPageWrapper() {
    return (
        <Suspense fallback={<Layout><p className="text-center mt-20">Carregando...</p></Layout>}>
            <ArtigoEditClient />
        </Suspense>
    );
}