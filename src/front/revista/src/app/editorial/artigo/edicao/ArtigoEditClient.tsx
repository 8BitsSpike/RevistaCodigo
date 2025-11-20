'use client';

import { useState, useEffect, Suspense, useMemo } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { useQuery, useMutation, ApolloError } from '@apollo/client';
import { OBTER_ARTIGO_EDITORIAL_VIEW, OBTER_STAFF_LIST, ATUALIZAR_CONTEUDO_ARTIGO, ATUALIZAR_METADADOS_ARTIGO, ADD_STAFF_COMENTARIO } from '@/graphql/queries';
import useAuth from '@/hooks/useAuth';
import Layout from '@/components/Layout';
import ProgressBar from '@/components/ProgressBar';
import StaffCommentCard from '@/components/StaffCommentCard';
import { StaffMember } from '@/components/StaffCard';
import { PosicaoEditorial, VersaoArtigo } from '@/types/enums';
import { StaffComentario } from '@/types/index';
import toast from 'react-hot-toast';

// --- Utility Function to clean route parameters ---
const sanitizeId = (id: string) => {
    if (!id) return '';
    // 1. Decode URL encoding
    let cleaned = decodeURIComponent(id);
    // 2. Remove URL parameter prefixes (e.g., "id=")
    if (cleaned.toLowerCase().startsWith('id=')) {
        cleaned = cleaned.substring(cleaned.indexOf('=') + 1);
    }
    // 3. Remove leading/trailing quotes/whitespace
    return cleaned.trim().replace(/^['"]|['"]$/g, '');
};


import Image from 'next/image';
import { User, Send } from 'lucide-react';
import StaffControlBar from '@/components/StaffControlBar';
import CreateCommentCard from '@/components/CreateCommentCard';
import EditorialQuillEditor from '@/components/EditorialQuillEditor';
import type { Range } from 'quill';

interface EditorialTeamData { initialAuthorId: string[]; editorId: string; reviewerIds: string[]; correctorIds: string[]; __typename: "EditorialTeam"; }
export interface ArtigoData { id: string; titulo: string; resumo: string; tipo: any; status: any; permitirComentario: boolean; editorialId: string; editorial: { position: PosicaoEditorial; team: EditorialTeamData; __typename: "EditorialView"; }; }
interface EditorialViewData { obterArtigoEditorialView: ArtigoData & { autorIds: string[]; editorial: { currentHistoryId: string; }; conteudoAtual: { version: VersaoArtigo; content: string; midias: { idMidia: string; url: string; textoAlternativo: string; }[]; staffComentarios: StaffComentario[]; __typename: "ArtigoHistoryEditorialView"; }; interacoes: { comentariosEditoriais: any[]; __typename: "InteractionConnectionDTO"; }; __typename: "ArtigoEditorialView"; }; }
interface StaffQueryData { obterStaffList: StaffMember[]; }

const TeamHeader = ({ team, staffList }: { team: EditorialTeamData | undefined, staffList: StaffMember[] }) => {
    // FIX: Added safety check for undefined team
    if (!team) return <div className="p-4 text-gray-500">Equipe não definida</div>;

    const allTeamIds = [...(team.initialAuthorId || []), ...(team.reviewerIds || []), ...(team.correctorIds || []), team.editorId].filter(Boolean);
    const teamMembers = allTeamIds.map(id => staffList.find(s => s?.usuarioId === id)).filter((s): s is StaffMember => !!s);

    return (
        <div className="flex flex-wrap gap-x-12 gap-y-4 mb-6 px-2 py-4 border-b border-gray-200">
            {teamMembers.map(member => (
                <div key={member.usuarioId} className="group relative flex flex-col items-center w-[50px]">
                    <div className="relative h-[25px] w-[25px] rounded-full overflow-hidden transition-all duration-300 opacity-40 group-hover:opacity-100 group-hover:rounded-none group-hover:h-[50px] group-hover:w-[50px]">
                        <Image src={member.url || '/faviccon.png'} alt={member.nome} fill className="object-cover" />
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

function ArtigoEditClient() {
    const router = useRouter();
    const searchParams = useSearchParams();
    const rawArtigoId = searchParams.get('Id'); const artigoId = rawArtigoId ? sanitizeId(rawArtigoId) : undefined;
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

    const { data: staffData, loading: loadingStaff } = useQuery<StaffQueryData>(OBTER_STAFF_LIST, {
        variables: { page: 0, pageSize: 200 },
        fetchPolicy: 'cache-and-network',
        onError: (err: ApolloError) => { if (err.graphQLErrors.some(e => e.extensions?.code === 'AUTH_FORBIDDEN')) { localStorage.removeItem('isStaff'); logout(); router.push('/'); } },
        onCompleted: (data) => { if (data.obterStaffList.find(s => s?.usuarioId === user?.id)?.isActive) setIsStaff(true); }
    });
    const staffList = staffData?.obterStaffList?.filter((s): s is StaffMember => !!s) ?? [];

    const { data, loading, refetch } = useQuery<EditorialViewData>(OBTER_ARTIGO_EDITORIAL_VIEW, {
        variables: { artigoId }, skip: !artigoId || !user, fetchPolicy: 'network-only',
        onCompleted: (data) => {
            const conteudo = data.obterArtigoEditorialView?.conteudoAtual;
            // FIX: Safety checks for potentially missing data
            if (data.obterArtigoEditorialView && conteudo) {
                setEditTitle(data.obterArtigoEditorialView.titulo || '');
                setEditResumo(data.obterArtigoEditorialView.resumo || '');
                setEditContent(conteudo.content || '');
                setEditMidias(conteudo.midias ? conteudo.midias.map(m => ({ midiaID: m.idMidia, url: m.url, alt: m.textoAlternativo })) : []);
            }
        },
        onError: (err) => { toast.error(err.message); if (err.graphQLErrors.some(e => e.extensions?.code === 'AUTH_FORBIDDEN')) router.push('/'); }
    });

    const artigo = data?.obterArtigoEditorialView;
    const editorial = artigo?.editorial;
    const conteudo = artigo?.conteudoAtual;

    const [addStaffComment, { loading: loadingAddComment }] = useMutation(ADD_STAFF_COMENTARIO, { onCompleted: () => { toast.success('Salvo!'); setNewStaffComment(''); setSelectedQuillRange(null); refetch(); }, onError: (err) => toast.error(err.message) });
    const [atualizarConteudo, { loading: loadingContent }] = useMutation(ATUALIZAR_CONTEUDO_ARTIGO);
    const [atualizarMetadados, { loading: loadingMeta }] = useMutation(ATUALIZAR_METADADOS_ARTIGO);

    useEffect(() => {
        if (loadingStaff || loading || !artigo || !user) return;
        const team = editorial?.team;
        if (isStaff) { setUserRole('staff'); return; }
        // FIX: Safety checks for team being undefined
        if (team?.initialAuthorId?.includes(user.id)) { setUserRole('author'); return; }
        if (team?.reviewerIds?.includes(user.id) || team?.correctorIds?.includes(user.id) || team?.editorId === user.id) { setUserRole('team'); return; }
        toast.error("Sem permissão."); router.push('/');
    }, [user, isStaff, artigo, loading, loadingStaff, router, editorial?.team]);

    const mode = useMemo((): 'edit' | 'comment' => {
        if (!artigo || !editorial || !conteudo) return 'comment';
        if (editorial.position === PosicaoEditorial.ProntoParaPublicar && conteudo.version >= VersaoArtigo.TerceiraEdicao && isStaff) return 'edit';
        if (userRole === 'author') { if (editorial.position === PosicaoEditorial.ProntoParaPublicar || conteudo.staffComentarios.length > 0) return 'comment'; return 'edit'; }
        return 'comment';
    }, [userRole, artigo, editorial, conteudo, isStaff]);

    // Handlers (kept same as before)
    const handleTextSelect = (range: Range) => { if (range.length > 0) { setSelectedQuillRange(range); setActiveStaffComment(null); } else setSelectedQuillRange(null); };
    const handleHighlightClick = (comment: StaffComentario) => { setActiveStaffComment(comment); setSelectedQuillRange(null); };
    const handleContentChange = (html: string) => setEditContent(html);
    const handleMediaChange = (midia: { id: string; url: string; alt: string }) => setEditMidias(prev => [...prev, { midiaID: midia.id, url: midia.url, alt: midia.alt }]);
    const handleCreateStaffComment = () => {
        if (!newStaffComment.trim() || !selectedQuillRange || !editorial) return;
        const commentData = { selection: selectedQuillRange, comment: newStaffComment, date: new Date().toISOString(), commentId: `temp-${Date.now()}`, parent: null };
        addStaffComment({ variables: { historyId: editorial.currentHistoryId, comment: JSON.stringify(commentData), parent: null } });
    };
    const handleSaveAuthorChanges = () => {
        if (!artigo) return;
        const toastId = toast.loading('Salvando...');
        Promise.all([
            atualizarConteudo({ variables: { artigoId: artigo.id, newContent: editContent, midias: editMidias, commentary: "Update pelo autor" } }),
            atualizarMetadados({ variables: { id: artigo.id, input: { titulo: editTitle, resumo: editResumo, status: null, tipo: null, idsAutor: null, referenciasAutor: null, permitirComentario: null, posicao: null }, commentary: "Update metadados" } })
        ]).then(() => { toast.success('Salvo!', { id: toastId }); refetch(); }).catch((err) => toast.error(err.message, { id: toastId }));
    };

    if (loading || loadingStaff) return <Layout pageType="editorial"><div className="text-center mt-20">Carregando dados editoriais...</div></Layout>;
    if (!artigo || !editorial || !conteudo) return <Layout pageType="editorial"><div className="text-center mt-20 text-red-500">Dados do artigo incompletos.</div></Layout>;

    return (
        <Layout pageType="editorial">
            <ProgressBar currentVersion={conteudo.version} />
            <div className="relative" style={{ marginLeft: '40px', padding: '1% 0.5%', width: 'calc(100% - 40px)' }}>
                <TeamHeader team={editorial.team} staffList={staffList} />
                {userRole === 'staff' && <StaffControlBar artigoId={artigo.id} editorialId={artigo.editorialId} currentData={artigo as ArtigoData} staffList={staffList} onUpdate={refetch} />}
                <div className="flex gap-4 mt-6">
                    <div className="flex-1" style={{ width: (conteudo.version > 0 || mode === 'comment') ? '80%' : '100%' }}>
                        {mode === 'edit' ? (
                            <div className="space-y-4">
                                <div><label className="block text-lg font-semibold text-gray-700 mb-2">Título</label><input type="text" value={editTitle} onChange={(e) => setEditTitle(e.target.value)} className="input-std" /></div>
                                <div><label className="block text-lg font-semibold text-gray-700 mb-2">Resumo</label><textarea value={editResumo} onChange={(e) => setEditResumo(e.target.value)} className="input-std h-32 resize-none" /></div>
                                <EditorialQuillEditor mode="edit" initialContent={editContent} onContentChange={handleContentChange} onMediaChange={handleMediaChange} />
                            </div>
                        ) : (
                            <div><h3 className="text-xl font-semibold mb-4">Modo de Comentário</h3><EditorialQuillEditor mode="comment" initialContent={conteudo.content} staffComments={conteudo.staffComentarios} onTextSelect={handleTextSelect} onHighlightClick={handleHighlightClick} /></div>
                        )}
                        {userRole !== 'staff' && mode === 'edit' && <div className="flex justify-end gap-4 mt-6"><button onClick={() => refetch()} className="btn-secondary">Cancelar</button><button onClick={handleSaveAuthorChanges} disabled={loadingContent || loadingMeta} className="btn-primary">Salvar Alterações</button></div>}
                        {editorial.position === PosicaoEditorial.ProntoParaPublicar && <div className="mt-10 pt-6 border-t"><h3 className="text-2xl font-semibold mb-4">Comentário Editorial</h3>{artigo.interacoes.comentariosEditoriais.some(c => c.usuarioId === user?.id) ? <p className="text-gray-600">Comentário enviado.</p> : <CreateCommentCard artigoId={artigo.id} onCommentPosted={refetch} isEditorial={true} />}</div>}
                    </div>
                    {(mode === 'comment' || conteudo.version > 0) && (
                        <div style={{ width: '20%' }} className="flex-shrink-0">
                            <h4 className="text-lg font-semibold mb-4">Comentários</h4>
                            <div className="space-y-3 max-h-[80vh] overflow-y-auto p-2 bg-gray-50 rounded-md">
                                {selectedQuillRange && (
                                    <div className="p-3 bg-white border border-emerald-300 rounded-lg shadow-md">
                                        <textarea value={newStaffComment} onChange={(e) => setNewStaffComment(e.target.value)} className="input-std text-sm" placeholder="Comentário..." />
                                        <div className="flex justify-end gap-2 mt-2"><button onClick={() => setSelectedQuillRange(null)} className="btn-secondary text-xs">Cancel</button><button onClick={handleCreateStaffComment} className="btn-primary text-xs"><Send size={14} /></button></div>
                                    </div>
                                )}
                                {activeStaffComment && <StaffCommentCard comment={activeStaffComment} historyId={editorial.currentHistoryId} onClose={() => setActiveStaffComment(null)} onCommentChange={refetch} staffList={staffList} />}
                                {conteudo.staffComentarios.filter(c => !c.parent).map(comment => (
                                    <div key={comment.id} className="p-2 border-b border-gray-200"><p className="text-xs text-gray-700 truncate cursor-pointer hover:underline" onClick={() => handleHighlightClick(comment)}>{(() => { try { return JSON.parse(comment.comment).comment; } catch { return comment.comment; } })().substring(0, 100)}...</p></div>
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
    return <Suspense fallback={<Layout pageType="editorial"><div className="text-center mt-20">Carregando...</div></Layout>}><ArtigoEditClient /></Suspense>;
}